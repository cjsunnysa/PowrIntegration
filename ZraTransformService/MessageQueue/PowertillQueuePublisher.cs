using FluentResults;
using Microsoft.Extensions.Options;
using PowrIntegration.Dtos;
using PowrIntegration.Options;
using System.Collections.Immutable;
using System.Text.Json;

namespace PowrIntegration.MessageQueue;

public sealed class PowertillQueuePublisher(
    IOptions<PowertillOptions> powertillOptions,
    RabbitMqFactory factory)
{
    private readonly PowertillOptions _powertillOptions = powertillOptions.Value;
    private readonly RabbitMqFactory _factory = factory;

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

            return Result.Ok();
        }
        catch (Exception ex)
        {
            return Result.Fail(new ExceptionalError("An exception occured pushing Zra Import Items to the queue.", ex));
        }
    }
}
