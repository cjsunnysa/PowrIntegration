using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Options;
using PowrIntegration.PowertillService.Data;
using PowrIntegration.PowertillService.Powertill;
using PowrIntegration.Shared.MessageQueue;
using PowrIntegration.Shared.Observability;
using PowrIntegration.Shared.Options;

namespace PowrIntegration.PowertillService.MessageQueue;

internal class PowertillServiceRabbitMqFactory(
    IOptions<RabbitMqOptions> options,
    IServiceProvider services,
    IMetrics metrics,
    ILogger<PowertillServiceRabbitMqFactory> logger)
    : RabbitMqFactory<ZraQueuePublisher, BackOfficeQueueConsumer>(options, logger)
{
    private readonly IServiceProvider _services = services;
    private readonly IMetrics _metrics = metrics;

    public override async Task<ZraQueuePublisher> CreatePublisher(CancellationToken cancellationToken)
    {
        var channel = await GetChannel(Options.Publisher, cancellationToken);

        return new ZraQueuePublisher(
            channel,
            Options.Publisher,
            _metrics,
            _services.GetRequiredService<ILogger<ZraQueuePublisher>>());
    }

    public override async Task<BackOfficeQueueConsumer> CreateConsumer(CancellationToken cancellationToken)
    {
        var channel = await GetChannel(Options.Consumer, cancellationToken);

        return new BackOfficeQueueConsumer(
            channel,
            Options.Consumer,
            _services.GetRequiredService<IDbContextFactory<PowrIntegrationDbContext>>(),
            _services.GetRequiredService<PurchaseFileExport>(),
            _metrics,
            _services.GetRequiredService<ILogger<BackOfficeQueueConsumer>>());
    }
}
