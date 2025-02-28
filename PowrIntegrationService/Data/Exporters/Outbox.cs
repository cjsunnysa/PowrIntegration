using EFCore.BulkExtensions;
using Microsoft.EntityFrameworkCore;
using PowrIntegration.Shared.Extensions;
using PowrIntegrationService.MessageQueue;
using System.Collections.Immutable;

namespace PowrIntegrationService.Data.Exporters;

public sealed class Outbox(IDbContextFactory<PowrIntegrationDbContext> dbContextFactory, RabbitMqFactory messageQueueFactory, ILogger<Outbox> logger)
{
    private readonly IDbContextFactory<PowrIntegrationDbContext> _dbContextFactory = dbContextFactory;
    private readonly RabbitMqFactory _messageQueueFactory = messageQueueFactory;
    private readonly ILogger<Outbox> _logger = logger;

    public async Task PublishToQueue(CancellationToken cancellationToken)
    {
        try
        {
            var zraQueuePublisher = await _messageQueueFactory.CreatePublisher<ZraQueuePublisher>(cancellationToken);

            using var dbContext = await _dbContextFactory.CreateDbContextAsync(cancellationToken);

            var outboxItems = dbContext.OutboxItems.ToImmutableArray();

            var publishResult = await zraQueuePublisher.PublishOutboxItems(outboxItems, cancellationToken);

            publishResult.LogErrors(_logger);

            if (publishResult.IsSuccess)
            {
                await dbContext.BulkDeleteAsync(outboxItems, cancellationToken: cancellationToken);
            }

            await dbContext.SaveChangesAsync(cancellationToken);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "An error occurred processing the outbox.");
        }
    }
}

