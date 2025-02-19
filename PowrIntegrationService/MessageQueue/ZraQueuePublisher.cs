using FluentResults;
using PowrIntegrationService.Data.Entities;
using PowrIntegrationService.Dtos;
using PowrIntegrationService.Options;
using RabbitMQ.Client;
using System.Text.Json;

namespace PowrIntegrationService.MessageQueue;

public sealed class ZraQueuePublisher(IChannel channel, MessageQueueOptions options, ILogger<ZraQueuePublisher> logger) 
    : RabbitMqPublisher(channel, options, logger)
{
    public async Task<Result> PublishSavePluItem(PluItemDto item, CancellationToken cancellationToken)
    {
        try
        {
            using var stream = new MemoryStream();
            
            await JsonSerializer.SerializeAsync(stream, item, cancellationToken: cancellationToken);

            await Publish(QueueMessageType.ItemInsert, stream.ToArray(), cancellationToken);

            return Result.Ok();
        }
        catch (Exception ex)
        {
            return Result.Fail(new ExceptionalError($"An exception occured publishing insert item message for Plu Number: {item.PluNumber} to the queue.", ex));
        }
    }
}
