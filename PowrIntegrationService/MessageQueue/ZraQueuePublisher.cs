using FluentResults;
using PowrIntegration.Shared.MessageQueue;
using PowrIntegrationService.Data.Entities;
using PowrIntegrationService.Dtos;
using PowrIntegrationService.Options;
using RabbitMQ.Client;
using System.Text.Json;

namespace PowrIntegrationService.MessageQueue;

public sealed class ZraQueuePublisher(IChannel channel, MessageQueueOptions options, ILogger<ZraQueuePublisher> logger) 
    : RabbitMqPublisher(channel, options, logger)
{
    public async Task<Result> PublishOutboxItems(IEnumerable<OutboxItem> records, CancellationToken cancellationToken)
    {
        try
        {
            await BatchPublish(records, cancellationToken);

            return Result.Ok();
        }
        catch (Exception ex)
        {
            return Result.Fail(new ExceptionalError($"An exception occured pushing outbox records to the queue.", ex));
        }
    }    
}
