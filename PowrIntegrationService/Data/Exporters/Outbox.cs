using Microsoft.EntityFrameworkCore;
using PowrIntegrationService.Extensions;
using PowrIntegrationService.MessageQueue;
using System.Collections.Immutable;

namespace PowrIntegrationService.Data.Exporters;

public sealed class Outbox(IDbContextFactory<PowrIntegrationDbContext> dbContextFactory, RabbitMqFactory messageQueueFactory, ILogger<Outbox> logger)
{
    private readonly IDbContextFactory<PowrIntegrationDbContext> _dbContextFactory = dbContextFactory;
    private readonly RabbitMqFactory _messageQueueFactory = messageQueueFactory;
    private readonly ILogger<Outbox> _logger = logger;

    public async Task SendItemsToApiQueue(CancellationToken cancellationToken)
    {
        try
        {
            var zraQueuePublisher = await _messageQueueFactory.CreatePublisher<ZraQueuePublisher>(cancellationToken);

            using var dbContext = await _dbContextFactory.CreateDbContextAsync(cancellationToken);

            var outboxItems = dbContext.OutboxItems.ToImmutableArray();

            foreach (var outboxItem in outboxItems)
            {
                var publishResult = await zraQueuePublisher.PublishOutboxItem(outboxItem, cancellationToken);

                if (publishResult.IsFailed && outboxItem.FailureCount < 4)
                {
                    publishResult.LogErrors(_logger);

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
