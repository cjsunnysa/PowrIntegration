using RabbitMQ.Client;
using RabbitMQ.Client.Events;

namespace PowrIntegration.MessageQueue;

public class RabbitMqConsumer
{
    private readonly IChannel _channel;
    private readonly string _queueName;
    private readonly string _deadLetterExchangeName;
    private readonly string _deadLetterQueueName;
    private readonly string _deadLetterRoutingKey;
    private readonly Func<BasicDeliverEventArgs, CancellationToken, Task<bool>> _handleMessageFunction;
    private readonly ILogger<RabbitMqConsumer> _logger;

    public RabbitMqConsumer(IChannel channel, string queueName, Func<BasicDeliverEventArgs, CancellationToken, Task<bool>> handleMessageFunction, ILogger<RabbitMqConsumer> logger)
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
                { "x-dead-letter-routing-key", _deadLetterRoutingKey }
                },
                cancellationToken: cancellationToken);

            var consumer = new AsyncEventingBasicConsumer(_channel);

            consumer.ReceivedAsync += async (model, basicDeliverEventArgs) =>
            {
                try
                {
                    _logger.LogInformation("Received message: {basicDeliverEventArgs} from queue: {queue}", basicDeliverEventArgs, _channel.CurrentQueue);

                    var ack = await _handleMessageFunction(basicDeliverEventArgs, cancellationToken);

                    if (!ack)
                    {
                        await _channel.BasicRejectAsync(basicDeliverEventArgs.DeliveryTag, requeue: false, cancellationToken);

                        return;
                    }

                    await _channel.BasicAckAsync(basicDeliverEventArgs.DeliveryTag, multiple: false, cancellationToken);
                }
                catch (Exception ex)
                {
                    _logger.LogError(ex, "An error occurred processing a message from queue: {queue}", _channel.CurrentQueue);
                }
            };

            await _channel.BasicConsumeAsync(
                queue: _queueName,
                autoAck: false,
                consumer: consumer,
                cancellationToken: cancellationToken);

            _logger.LogInformation("RabbitMQ consumer is listening...");
        }
        catch(Exception ex)
        {
            _logger.LogError(ex, "An exception occurred.");
        }
    }
}
