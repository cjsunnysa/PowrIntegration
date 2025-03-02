using Microsoft.Extensions.Options;
using PowrIntegration.PowertillService.Data.Exporters;
using PowrIntegration.PowertillService.Data.Importers;
using PowrIntegration.PowertillService.MessageQueue;
using PowrIntegration.PowertillService.Options;
using PowrIntegration.PowertillService.Powertill;
using PowrIntegration.Shared.Observability;

namespace PowrIntegration.PowertillService;

internal sealed class Worker(
    IOptions<BackOfficeServiceOptions> serviceOptions,
    PowertillServiceRabbitMqFactory messageQueueFactory,
    PluItemsFileImport pluItemsImport,
    ClassificationCodesFileImport classificationImport,
    Outbox outbox,
    IMetrics metrics,
    ILogger<Worker> logger) : BackgroundService
{
    private readonly BackOfficeServiceOptions _serviceOptions = serviceOptions.Value;
    private readonly PowertillServiceRabbitMqFactory _messageQueueFactory = messageQueueFactory;
    private readonly Outbox _outbox = outbox;
    private readonly IMetrics _metrics = metrics;
    private readonly PluItemsFileImport _pluItemsFileImport = pluItemsImport;
    private readonly ClassificationCodesFileImport _classificationFileImport = classificationImport;
    private readonly ILogger<Worker> _logger = logger;

    protected override async Task ExecuteAsync(CancellationToken cancellationToken)
    {
        await _classificationFileImport.Execute(cancellationToken);

        await _pluItemsFileImport.Execute(cancellationToken);

        var queueConsumer = await _messageQueueFactory.CreateConsumer(cancellationToken);

        await queueConsumer.Start(cancellationToken);

        int serviceTimeoutMilliseconds = Convert.ToInt32(TimeSpan.FromSeconds(_serviceOptions.TimeoutSeconds).TotalMilliseconds);

        while (!cancellationToken.IsCancellationRequested)
        {
            try
            {
                if (_logger.IsEnabled(LogLevel.Information))
                {
                    _logger.LogInformation("{ApplicationName} service worker running at: {time}", _metrics.ApplicationName, DateTimeOffset.Now);
                }

                await PublishOutboxItems(cancellationToken);

                await Task.Delay(serviceTimeoutMilliseconds, cancellationToken);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "An exception occurred in the main main service loop.");
            }
        }
    }

    private async Task PublishOutboxItems(CancellationToken cancellationToken)
    {
        await _outbox.PublishToQueue(cancellationToken);
    }
}
