using FluentResults;
using PowrIntegration.PowertillService.Data.Entities;
using PowrIntegration.Shared.MessageQueue;
using PowrIntegration.Shared.Observability;
using PowrIntegration.Shared.Options;
using RabbitMQ.Client;

namespace PowrIntegration.PowertillService.MessageQueue;

public sealed class ZraQueuePublisher(IChannel channel, MessageQueueOptions options, IMetrics metrics, ILogger<ZraQueuePublisher> logger)
    : RabbitMqPublisher(channel, options, metrics.MetricsMeterName, logger)
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
