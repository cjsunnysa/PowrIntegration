using FluentResults;
using PowrIntegrationService.Data.Entities;
using PowrIntegrationService.Options;
using RabbitMQ.Client;

namespace PowrIntegrationService.MessageQueue;

public sealed class ZraQueuePublisher(IChannel channel, MessageQueueOptions options, ILogger<ZraQueuePublisher> logger) 
    : RabbitMqPublisher(channel, options, logger)
{
    public async Task<Result> PublishOutboxItem(OutboxItem record, CancellationToken cancellationToken)
    {
        try
        {
            await Publish(record.MessageType, record.MessageBody, cancellationToken);

            return Result.Ok();
        }
        catch (Exception ex)
        {
            return Result.Fail(new ExceptionalError($"An exception occured pushing outbox record Id: {record.Id} to the queue.", ex));
        }
    }
}
