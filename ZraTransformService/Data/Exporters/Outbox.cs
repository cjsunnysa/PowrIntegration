using Microsoft.EntityFrameworkCore;
using PowrIntegration.MessageQueue;
using System.Collections.Immutable;

namespace PowrIntegration.Data.Exporters;

public sealed class Outbox(IDbContextFactory<PowrIntegrationDbContext> dbContextFactory, ZraQueuePublisher zraQueuePublisher, ILogger<Outbox> logger)
{
    private readonly IDbContextFactory<PowrIntegrationDbContext> _dbContextFactory = dbContextFactory;
    private readonly ZraQueuePublisher _zraQueuePublisher = zraQueuePublisher;
    private readonly ILogger<Outbox> _logger = logger;

    public async Task SendItemsToApiQueue(CancellationToken cancellationToken)
    {
        try
        {
            using var dbContext = await _dbContextFactory.CreateDbContextAsync(cancellationToken);

            var outboxItems = dbContext.OutboxItems.ToImmutableArray();

            foreach (var outboxItem in outboxItems)
            {
                var publishResult = await _zraQueuePublisher.PublishOutboxItem(outboxItem, cancellationToken);

                if (publishResult.IsFailed && outboxItem.FailureCount < 4)
                {
                    _logger.LogError("Unable to publish outbox item to api message queue. Type: {MessageType}, OutboxId: {OutboxId}", outboxItem.MessageType, outboxItem.Id);

                    outboxItem.FailureCount++;
                }
                else
                {
                    dbContext.OutboxItems.Remove(outboxItem);
                }

            }
            
            await dbContext.SaveChangesAsync(cancellationToken);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "An error occurred processing the outbox.");
        }
    }
}
