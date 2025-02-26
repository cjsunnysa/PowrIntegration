using PowrIntegration.Shared;
using PowrIntegration.Shared.Extensions;
using PowrIntegration.Shared.MessageQueue;
using PowrIntegrationService.Extensions;
using PowrIntegrationService.Options;
using RabbitMQ.Client;
using RabbitMQ.Client.Events;
using System.Diagnostics.Metrics;

namespace PowrIntegrationService.MessageQueue;

public abstract class RabbitMqConsumer
{
    public enum MessageAction
    {
        Acknowledge = 1,
        Requeue = 2,
        Reject = 3
    }

    protected readonly MessageQueueOptions Options;
    private readonly IChannel _channel;
    private readonly ILogger _logger;
    private readonly Counter<long> _messagesAcknowledgedCounter;
    private readonly Counter<long> _messagesRequeuedCounter;
    private readonly Counter<long> _messagesRejectedCounter;

    public RabbitMqConsumer(IChannel channel, MessageQueueOptions options, ILogger logger)
    {
        Options = options;
        _channel = channel;
        _logger = logger;

        var meter = new Meter(PowrIntegrationValues.MetricsMeterName);
        
        var queueLabel = options.Name.ToSnakeCase();
                
        _messagesAcknowledgedCounter = meter.CreateCounter<long>($"{queueLabel}_messages_acknowledged", "messages", "Number of messages acknowledged.");
        _messagesRequeuedCounter = meter.CreateCounter<long>($"{queueLabel}_messages_requeued", "messages", "Number of messages requeued.");
        _messagesRejectedCounter = meter.CreateCounter<long>($"{queueLabel}_messages_rejected", "messages", "Number of messages rejected.");
    }

    protected abstract Task<MessageAction> HandleMessage(QueueMessageType type, ReadOnlyMemory<byte> messageBody, CancellationToken cancellationToken);

    public async Task Start(CancellationToken cancellationToken)
    {
        try
        {
            await _channel.ExchangeDeclareAsync(
                exchange: Options.DeadLetterQueue.ExchangeName,
                type: ExchangeType.Direct,
                durable: true,
                autoDelete: false,
                cancellationToken: cancellationToken);

            await _channel.QueueDeclareAsync(
                queue: Options.DeadLetterQueue.Name,
                durable: true,
                exclusive: false,
                cancellationToken: cancellationToken);

            await _channel.QueueBindAsync(
                queue: Options.DeadLetterQueue.Name,
                exchange: Options.DeadLetterQueue.ExchangeName,
                routingKey: Options.DeadLetterQueue.RoutingKey,
                cancellationToken: cancellationToken);

            await _channel.QueueDeclareAsync(
                queue: Options.Name,
                durable: true,
                exclusive: false,
                autoDelete: false,
                arguments: new Dictionary<string, object?>
                {
                    { "x-dead-letter-exchange", Options.DeadLetterQueue.ExchangeName },
                    { "x-dead-letter-routing-key", Options.DeadLetterQueue.RoutingKey },
                    { "x-max-priority", 2 }
                },
                cancellationToken: cancellationToken);

            var consumer = new AsyncEventingBasicConsumer(_channel);

            consumer.ReceivedAsync += OnMessageReceived;

            await _channel.BasicConsumeAsync(
                queue: Options.Name,
                autoAck: false,
                consumer: consumer,
                cancellationToken: cancellationToken);

            _logger.LogInformation("Queue {QueueName} consumer is listening...", Options.Name);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "An error occurred configuring queue {QueueName}.", Options.Name);
        }
    }

    private async Task OnMessageReceived(object model, BasicDeliverEventArgs basicDeliverEventArgs)
    {
        QueueMessageType? messageType = null;

        try
        {
            var messageId = basicDeliverEventArgs.BasicProperties.MessageId;

            _logger.LogInformation("Received message with MessageId: {MessageId} from queue: {QueueName}", messageId, Options.Name);

            var getMessageTypeResult = basicDeliverEventArgs.GetQueueMessageType();

            if (getMessageTypeResult.IsFailed)
            {
                getMessageTypeResult.LogErrors(_logger);

                _messagesRejectedCounter.Add(1, new KeyValuePair<string, object?>("type", "unkown_message_type"));

                await _channel.BasicRejectAsync(basicDeliverEventArgs.DeliveryTag, false, CancellationToken.None);

                return;
            }

            messageType = getMessageTypeResult.Value;

            var messageBody = new byte[basicDeliverEventArgs.Body.Length];

            basicDeliverEventArgs.Body.CopyTo(messageBody);

            MessageAction action = await HandleMessage(messageType.Value, messageBody, CancellationToken.None);

            if (action is MessageAction.Acknowledge)
            {
                _logger.LogInformation("Successfully processed message with MessageId: {MessageId} from queue: {QueueName}", messageId, Options.Name);

                _messagesAcknowledgedCounter.Add(1, new KeyValuePair<string, object?>("type", messageType.Value.ToLabel()));

                await _channel.BasicAckAsync(basicDeliverEventArgs.DeliveryTag, multiple: false, CancellationToken.None);

                return;
            }

            if (action is MessageAction.Requeue)
            {
                _logger.LogInformation("Requeueing message with MessageId: {MessageId} from queue: {QueueName}", messageId, Options.Name);

                _messagesRequeuedCounter.Add(1, new KeyValuePair<string, object?>("type", messageType.Value.ToLabel()));

                await _channel.BasicRejectAsync(basicDeliverEventArgs.DeliveryTag, requeue: true, CancellationToken.None);

                return;
            }

            _logger.LogInformation("Rejected message with MessageId: {MessageId} from queue: {QueueName}", messageId, Options.Name);

            _messagesRejectedCounter.Add(1, new KeyValuePair<string, object?>("type", messageType.Value.ToLabel()));

            await _channel.BasicRejectAsync(basicDeliverEventArgs.DeliveryTag, requeue: false, CancellationToken.None);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "An error occurred consuming a message from queue: {QueueName} message queue.", Options.Name);

            _messagesRejectedCounter.Add(1, new KeyValuePair<string, object?>("type", messageType.HasValue ? messageType.Value : "unknown_message_type"));

            await _channel.BasicRejectAsync(basicDeliverEventArgs.DeliveryTag, requeue: false, CancellationToken.None);
        }
    }
}
