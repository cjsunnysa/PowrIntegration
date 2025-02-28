using FluentResults;
using PowrIntegration.Shared.MessageQueue;
using PowrIntegration.Shared.Options;
using PowrIntegrationService.Data.Entities;
using RabbitMQ.Client;

namespace PowrIntegrationService.MessageQueue;

public sealed class ZraQueuePublisher(IChannel channel, MessageQueueOptions options, ILogger<ZraQueuePublisher> logger) 
    : RabbitMqPublisher(channel, options, Metrics.MetricsMeterName, logger)
{
    public async Task<Result> PublishOutboxItems(IEnumerable<OutboxItem> records, CancellationToken cancellationToken)
    {
        try
        {
            var groups = records.GroupBy(x => x.MessageType);
            
            await BatchPublish(groups, cancellationToken);

            return Result.Ok();
        }
        catch (Exception ex)
        {
            return Result.Fail(new ExceptionalError($"An exception occured pushing outbox records to the queue.", ex));
        }
    }    
}
