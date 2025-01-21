using FluentResults;
using Microsoft.Extensions.Options;
using PowrIntegrationService.Dtos;
using PowrIntegrationService.Options;
using System.Collections.Immutable;
using System.Diagnostics.Metrics;
using System.Text.Json;

namespace PowrIntegrationService.MessageQueue;

public sealed class PowertillQueuePublisher
{
    private readonly PowertillOptions _powertillOptions;
    private readonly RabbitMqFactory _factory;
    private readonly Counter<long> _messagesPublishedCounter;

    public PowertillQueuePublisher(
        IOptions<PowertillOptions> powertillOptions,
        RabbitMqFactory factory)
    {
        _powertillOptions = powertillOptions.Value;
        _factory = factory;

        var meter = new Meter(PowrIntegrationValues.MetricsMeterName);
        _messagesPublishedCounter = meter.CreateCounter<long>("powertill_queue_messages_published", "messages", "Number of messages published.");
    }

    public async Task<Result> PublishStandardCodes(ImmutableArray<StandardCodeClassDto> dtos, CancellationToken cancellationToken)
    {
        try
        {
            if (dtos.Length == 0)
            {
                return Result.Ok();
            }

            var queuePublisher = await _factory.CreatePublisher(_powertillOptions.QueueHost, _powertillOptions.QueueName, cancellationToken);

            using var memoryStream = new MemoryStream();

            await JsonSerializer.SerializeAsync(memoryStream, dtos, cancellationToken: cancellationToken);

            var messageBytes = memoryStream?.ToArray() ?? [];

            await queuePublisher.Publish(QueueMessageType.StandardCodes, messageBytes, cancellationToken);

            _messagesPublishedCounter.Add(1, KeyValuePair.Create<string, object?>("type", "standard_codes"));

            return Result.Ok();
        }
        catch (Exception ex)
        {
            return Result.Fail(new ExceptionalError("An exception occured pushing Standard Codes to the queue.", ex));
        }
    }

    public async Task<Result> PublishClassificationCodes(ImmutableArray<ClassificationCodeDto> dtos, CancellationToken cancellationToken)
    {
        try
        {
            if (dtos.Length == 0)
            {
                return Result.Ok();
            }

            var queuePublisher = await _factory.CreatePublisher(_powertillOptions.QueueHost, _powertillOptions.QueueName, cancellationToken);

            using var memoryStream = new MemoryStream();

            await JsonSerializer.SerializeAsync(memoryStream, dtos, cancellationToken: cancellationToken);

            var messageBytes = memoryStream?.ToArray() ?? [];

            await queuePublisher.Publish(QueueMessageType.ClassificationCodes, messageBytes, cancellationToken);

            _messagesPublishedCounter.Add(1, KeyValuePair.Create<string, object?>("type", "classification_codes"));

            return Result.Ok();
        }
        catch (Exception ex)
        {
            return Result.Fail(new ExceptionalError("An exception occured pushing Classification Codes to the queue.", ex));
        }
    }

    public async Task<Result> PublishZraImportItems(ImmutableArray<ImportItemDto> dtos, CancellationToken cancellationToken)
    {
        try
        {
            if (dtos.Length == 0)
            {
                return Result.Ok();
            }

            var queuePublisher = await _factory.CreatePublisher(_powertillOptions.QueueHost, _powertillOptions.QueueName, cancellationToken);

            using var memoryStream = new MemoryStream();

            await JsonSerializer.SerializeAsync(memoryStream, dtos, cancellationToken: cancellationToken);

            var messageBytes = memoryStream?.ToArray() ?? [];

            await queuePublisher.Publish(QueueMessageType.ZraImportItems, messageBytes, cancellationToken);

            _messagesPublishedCounter.Add(1, KeyValuePair.Create<string, object?>("type", "zra_imported_items"));

            return Result.Ok();
        }
        catch (Exception ex)
        {
            return Result.Fail(new ExceptionalError("An exception occured pushing Zra Import Items to the queue.", ex));
        }
    }
}
