using FluentResults;
using PowrIntegration.BackOfficeService.Data.Entities;
using PowrIntegration.Shared.MessageQueue;
using PowrIntegration.Shared.Observability;
using PowrIntegration.Shared.Options;
using RabbitMQ.Client;

namespace PowrIntegration.BackOfficeService.MessageQueue;

public sealed class ZraQueuePublisher(IChannel channel, MessageQueueOptions options, IMetrics metrics, ILogger<ZraQueuePublisher> logger)
    : RabbitMqPublisher(channel, options, metrics.MetricsMeterName, logger)
{
    public async Task<Result> PublishOutboxItems(IEnumerable<OutboxItem> records, CancellationToken cancellationToken)
    {
        try
        {
            IEnumerable<IGrouping<QueueMessageType, byte[]>> groups = records.Select(x => (x.MessageType, x.MessageBody)).GroupBy(x => x.MessageType, x => x.MessageBody);

            await BatchPublish(groups, cancellationToken);

            return Result.Ok();
        }
        catch (Exception ex)
        {
            return Result.Fail(new ExceptionalError($"An exception occured pushing outbox records to the queue.", ex));
        }
    }
}
