using Microsoft.Extensions.Options;
using PowrIntegration.Shared.MessageQueue;
using PowrIntegration.Shared.Observability;
using PowrIntegration.Shared.Options;
using PowrIntegration.ZraService.Zra;

namespace PowrIntegration.ZraService.MessageQueue;

public class ZraServiceRabbitMqFactory(
    IOptions<RabbitMqOptions> options,
    IServiceProvider services,
    IMetrics metrics,
    ILogger<ZraServiceRabbitMqFactory> logger)
    : RabbitMqFactory<BackOfficeQueuePublisher, ZraQueueConsumer>(options, logger)
{
    private readonly IServiceProvider _services = services;
    private readonly IMetrics _metrics = metrics;

    public override async Task<BackOfficeQueuePublisher> CreatePublisher(CancellationToken cancellationToken)
    {
        var channel = await GetChannel(Options.Publisher, cancellationToken);

        return new BackOfficeQueuePublisher(
            channel,
            Options.Publisher,
            _metrics,
            _services.GetRequiredService<ILogger<BackOfficeQueuePublisher>>());
    }

    public override async Task<ZraQueueConsumer> CreateConsumer(CancellationToken cancellationToken)
    {
        var channel = await GetChannel(Options.Consumer, cancellationToken);

        return new ZraQueueConsumer(
            channel,
            Options.Consumer,
            _services.GetRequiredService<ZraRestService>(),
            _metrics,
            _services.GetRequiredService<ILogger<ZraQueueConsumer>>());
    }
}