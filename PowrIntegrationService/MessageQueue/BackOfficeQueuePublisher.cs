using FluentResults;
using PowrIntegrationService.Dtos;
using PowrIntegrationService.Options;
using RabbitMQ.Client;
using System.Collections.Immutable;
using System.Text.Json;

namespace PowrIntegrationService.MessageQueue;

public sealed class BackOfficeQueuePublisher(
    IChannel channel,
    MessageQueueOptions options,
    ILogger<BackOfficeQueuePublisher> logger) : RabbitMqPublisher(channel, options, logger)
{
    public async Task<Result> PublishStandardCodes(ImmutableArray<StandardCodeClassDto> dtos, CancellationToken cancellationToken)
    {
        try
        {
            if (dtos.Length == 0)
            {
                return Result.Ok();
            }

            using var memoryStream = new MemoryStream();

            await JsonSerializer.SerializeAsync(memoryStream, dtos, cancellationToken: cancellationToken);

            var messageBytes = memoryStream?.ToArray() ?? [];

            await Publish(QueueMessageType.ZraStandardCodes, messageBytes, cancellationToken);

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

            using var memoryStream = new MemoryStream();

            await JsonSerializer.SerializeAsync(memoryStream, dtos, cancellationToken: cancellationToken);

            var messageBytes = memoryStream?.ToArray() ?? [];

            await Publish(QueueMessageType.ZraClassificationCodes, messageBytes, cancellationToken);

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

            using var memoryStream = new MemoryStream();

            await JsonSerializer.SerializeAsync(memoryStream, dtos, cancellationToken: cancellationToken);

            var messageBytes = memoryStream?.ToArray() ?? [];

            await Publish(QueueMessageType.ZraImportItems, messageBytes, cancellationToken);

            return Result.Ok();
        }
        catch (Exception ex)
        {
            return Result.Fail(new ExceptionalError("An exception occured pushing Zra Import Items to the queue.", ex));
        }
    }
}
