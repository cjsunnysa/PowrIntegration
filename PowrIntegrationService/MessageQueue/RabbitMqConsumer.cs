using RabbitMQ.Client;
using RabbitMQ.Client.Events;

namespace PowrIntegrationService.MessageQueue;

public class RabbitMqConsumer
{
    public enum MessageAction
    {
        Acknowledge = 1,
        Requeue = 2,
        Reject = 3
    }

    private readonly IChannel _channel;
    private readonly string _queueName;
    private readonly string _deadLetterExchangeName;
    private readonly string _deadLetterQueueName;
    private readonly string _deadLetterRoutingKey;
    private readonly Func<BasicDeliverEventArgs, CancellationToken, Task<MessageAction>> _handleMessageFunction;
    private readonly ILogger<RabbitMqConsumer> _logger;

    public RabbitMqConsumer(IChannel channel, string queueName, Func<BasicDeliverEventArgs, CancellationToken, Task<MessageAction>> handleMessageFunction, ILogger<RabbitMqConsumer> logger)
    {
        _channel = channel;
        _queueName = queueName;
        _deadLetterExchangeName = $"{queueName}_dlx";
        _deadLetterQueueName = $"{queueName}Dead";
        _deadLetterRoutingKey = $"{queueName.ToLower()}-dead";
        _handleMessageFunction = handleMessageFunction;
        _logger = logger;
    }

    public async Task Start(CancellationToken cancellationToken)
    {
        try
        {
            await _channel.ExchangeDeclareAsync(
                exchange: _deadLetterExchangeName,
                type: ExchangeType.Direct,
                durable: true,
                autoDelete: false,
                cancellationToken: cancellationToken);

            await _channel.QueueDeclareAsync(
                queue: _deadLetterQueueName,
                durable: true,
                exclusive: false,
                cancellationToken: cancellationToken);

            await _channel.QueueBindAsync(
                queue: _deadLetterQueueName,
                exchange: _deadLetterExchangeName,
                routingKey: _deadLetterRoutingKey,
                cancellationToken: cancellationToken);

            await _channel.QueueDeclareAsync(
                queue: _queueName,
                durable: true,
                exclusive: false,
                autoDelete: false,
                arguments: new Dictionary<string, object?>
                {
                    { "x-dead-letter-exchange", _deadLetterExchangeName },
                    { "x-dead-letter-routing-key", _deadLetterRoutingKey },
                    { "x-max-priority", 2 }
                },
                cancellationToken: cancellationToken);

            var consumer = new AsyncEventingBasicConsumer(_channel);

            consumer.ReceivedAsync += async (model, basicDeliverEventArgs) =>
            {
                var messageId = basicDeliverEventArgs.BasicProperties.MessageId;

                _logger.LogInformation("Received message with MessageId: {MessageId} from queue: {QueueName}", messageId, _queueName);

                MessageAction action = await _handleMessageFunction(basicDeliverEventArgs, cancellationToken);

                if (action is MessageAction.Acknowledge)
                {
                    _logger.LogInformation("Successfully processed message with MessageId: {MessageId} from queue: {QueueName}", messageId, _queueName);

                    await _channel.BasicAckAsync(basicDeliverEventArgs.DeliveryTag, multiple: false, cancellationToken);

                    return;
                }

                if (action is MessageAction.Requeue)
                {
                    _logger.LogInformation("Requeueing message with MessageId: {MessageId} from queue: {QueueName}", messageId, _queueName);

                    await _channel.BasicRejectAsync(basicDeliverEventArgs.DeliveryTag, requeue: true, cancellationToken);

                    return;
                }

                _logger.LogInformation("Rejected message with MessageId: {MessageId} from queue: {QueueName}", messageId, _queueName);

                await _channel.BasicRejectAsync(basicDeliverEventArgs.DeliveryTag, requeue: false, cancellationToken);
            };

            await _channel.BasicConsumeAsync(
                queue: _queueName,
                autoAck: false,
                consumer: consumer,
                cancellationToken: cancellationToken);

            _logger.LogInformation("RabbitMQ consumer is listening...");
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "An error occurred configuring queue {QueueName}.", _queueName);
        }
    }
}
