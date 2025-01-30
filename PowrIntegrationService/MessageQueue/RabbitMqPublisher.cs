using PowrIntegrationService.Extensions;
using PowrIntegrationService.Options;
using RabbitMQ.Client;
using System.Diagnostics.Metrics;

namespace PowrIntegrationService.MessageQueue;

public abstract class RabbitMqPublisher
{
    protected readonly MessageQueueOptions Options;
    private readonly IChannel _channel;
    private readonly ILogger _logger;
    private readonly Counter<long> _messagesPublishedCounter;

    public RabbitMqPublisher(IChannel channel, MessageQueueOptions options, ILogger logger)
    {
        Options = options;
        _channel = channel;
        _logger = logger;

        var meter = new Meter(PowrIntegrationValues.MetricsMeterName);

        _messagesPublishedCounter = meter.CreateCounter<long>($"{options.Name.ToSnakeCase()}_messages_published", "messages", "Number of messages published.");
    }

    protected async Task Publish(QueueMessageType messageType, byte[] message, CancellationToken cancellationToken)
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

        var properties = new BasicProperties
        {
            Persistent = true,
            MessageId = Guid.NewGuid().ToString(),
            Priority = (byte)(messageType == QueueMessageType.ItemInsert ? 2 : 1),
            Headers = new Dictionary<string, object?>
            {
                { "Type", Enum.GetName(messageType) },
            },
        };

        await _channel.BasicPublishAsync(exchange: "", routingKey: Options.Name, mandatory: true, basicProperties: properties, body: message, cancellationToken: cancellationToken);

        _messagesPublishedCounter.Add(1, new KeyValuePair<string, object?>("type", messageType.ToLabel()));

        _logger.LogInformation("Message published to queueName: {QueueName}, Id: {MessageId}, MessageType: {MessageType}", Options.Name, properties.MessageId, Enum.GetName(messageType));
    }
}