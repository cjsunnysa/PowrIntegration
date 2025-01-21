using RabbitMQ.Client;

namespace PowrIntegrationService.MessageQueue;

public class RabbitMqPublisher
{
    private readonly IChannel _channel;
    private readonly string _queueName;
    private readonly string _deadLetterExchangeName;
    private readonly string _deadLetterQueueName;
    private readonly string _deadLetterRoutingKey;
    private readonly ILogger _logger;

    public RabbitMqPublisher(IChannel channel, string queueName, ILogger logger)
    {
        _channel = channel;
        _queueName = queueName;
        _deadLetterExchangeName = $"{queueName}_dlx";
        _deadLetterQueueName = $"{queueName}Dead";
        _deadLetterRoutingKey = $"{queueName.ToLower()}-dead";
        _logger = logger;
    }

    public async Task Publish(QueueMessageType messageType, byte[] message, CancellationToken cancellationToken)
    {
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

        await _channel.BasicPublishAsync(exchange: "", routingKey: _queueName, mandatory: true, basicProperties: properties, body: message, cancellationToken: cancellationToken);

        _logger.LogInformation("Message published to queueName: {QueueName}, Id: {MessageId}, MessageType: {MessageType}", _queueName, properties.MessageId, Enum.GetName(messageType));
    }
}