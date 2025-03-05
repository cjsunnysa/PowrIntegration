using Microsoft.Extensions.Logging;
using PowrIntegration.Shared.Extensions;
using PowrIntegration.Shared.Options;
using RabbitMQ.Client;
using System.Diagnostics.Metrics;
using System.Text.Json;

namespace PowrIntegration.Shared.MessageQueue;

public abstract class RabbitMqPublisher
{
    protected readonly MessageQueueOptions Options;
    private readonly IChannel _channel;
    private readonly ILogger _logger;
    private readonly Counter<long> _messagesPublishedCounter;

    public RabbitMqPublisher(IChannel channel, MessageQueueOptions options, string metricsMeterName, ILogger logger)
    {
        Options = options;
        _channel = channel;
        _logger = logger;

        var meter = new Meter(metricsMeterName);

        _messagesPublishedCounter = meter.CreateCounter<long>($"{options.Name.ToSnakeCase()}_messages_published", "messages", "Number of messages published.");
    }

    protected async Task Publish(QueueMessageType messageType, byte[] message, CancellationToken cancellationToken)
    {
        await EnsureQueueCreated(cancellationToken);

        BasicProperties properties = CreateBasicMessageProperties(messageType);

        await _channel.BasicPublishAsync(exchange: "", routingKey: Options.Name, mandatory: true, basicProperties: properties, body: message, cancellationToken: cancellationToken);

        _messagesPublishedCounter.Add(1, new KeyValuePair<string, object?>("type", messageType.ToLabel()));

        _logger.LogInformation("Message published to queueName: {QueueName}, Id: {MessageId}, MessageType: {MessageType}", Options.Name, properties.MessageId, Enum.GetName(messageType));
    }

    protected async Task BatchPublish<T>(QueueMessageType messageType, IEnumerable<T> records, CancellationToken cancellationToken)
    {
        await EnsureQueueCreated(cancellationToken);

        await _channel.TxSelectAsync(cancellationToken);

        try
        {
            await Publish(messageType, records, cancellationToken);

            await _channel.TxCommitAsync(cancellationToken);
        }
        catch (Exception)
        {
            throw;
        }
        finally
        {
            await _channel.TxRollbackAsync(cancellationToken);
        }
    }

    protected async Task BatchPublish(IEnumerable<IGrouping<QueueMessageType, byte[]>> groups, CancellationToken cancellationToken)
    {
        await EnsureQueueCreated(cancellationToken);

        await _channel.TxSelectAsync(cancellationToken);

        try
        {
            foreach (var group in groups)
            {
                await Publish(group.Key, bodyRecords: group, cancellationToken);
            }

            await _channel.TxCommitAsync(cancellationToken);
        }
        catch (Exception)
        {
            throw;
        }
        finally
        {
            await _channel.TxRollbackAsync(cancellationToken);
        }
    }

    private async Task Publish(QueueMessageType messageType, IEnumerable<byte[]> bodyRecords, CancellationToken cancellationToken)
    {
        foreach (var record in bodyRecords)
        {
            BasicProperties properties = CreateBasicMessageProperties(messageType);

            using var stream = new MemoryStream();

            await _channel.BasicPublishAsync(exchange: "", routingKey: Options.Name, mandatory: true, basicProperties: properties, body: record, cancellationToken);
        }
    }

    private async Task Publish<T>(QueueMessageType messageType, IEnumerable<T> records, CancellationToken cancellationToken)
    {
        foreach (var record in records)
        {
            BasicProperties properties = CreateBasicMessageProperties(messageType);

            using var stream = new MemoryStream();

            await JsonSerializer.SerializeAsync(stream, record, cancellationToken: cancellationToken);

            await _channel.BasicPublishAsync(exchange: "", routingKey: Options.Name, mandatory: true, basicProperties: properties, body: stream.ToArray(), cancellationToken);
        }
    }

    private static BasicProperties CreateBasicMessageProperties(QueueMessageType messageType)
    {
        return new BasicProperties
        {
            Persistent = true,
            MessageId = Guid.NewGuid().ToString(),
            Priority = (byte)(messageType == QueueMessageType.ItemInsert ? 2 : 1),
            Headers = new Dictionary<string, object?>
            {
                { MessageQueueHeaderKey.Type, Enum.GetName(messageType) },
            }
        };
    }

    private async Task EnsureQueueCreated(CancellationToken cancellationToken)
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
    }
}