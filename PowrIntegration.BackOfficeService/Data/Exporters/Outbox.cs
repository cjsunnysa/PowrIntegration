using EFCore.BulkExtensions;
using Microsoft.EntityFrameworkCore;
using PowrIntegration.BackOfficeService.Data;
using PowrIntegration.BackOfficeService.MessageQueue;
using PowrIntegration.Shared.Extensions;
using System.Collections.Immutable;

namespace PowrIntegration.BackOfficeService.Data.Exporters;

internal sealed class Outbox(IDbContextFactory<PowrIntegrationDbContext> dbContextFactory, PowertillServiceRabbitMqFactory messageQueueFactory, ILogger<Outbox> logger)
{
    private readonly IDbContextFactory<PowrIntegrationDbContext> _dbContextFactory = dbContextFactory;
    private readonly PowertillServiceRabbitMqFactory _messageQueueFactory = messageQueueFactory;
    private readonly ILogger<Outbox> _logger = logger;

    public async Task PublishToQueue(CancellationToken cancellationToken)
    {
        try
        {
            var zraQueuePublisher = await _messageQueueFactory.CreatePublisher(cancellationToken);

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

