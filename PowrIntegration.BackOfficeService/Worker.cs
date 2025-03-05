using Microsoft.Extensions.Options;
using PowrIntegration.BackOfficeService.Data.Exporters;
using PowrIntegration.BackOfficeService.Data.Importers;
using PowrIntegration.BackOfficeService.MessageQueue;
using PowrIntegration.BackOfficeService.Options;
using PowrIntegration.BackOfficeService.Powertill;
using PowrIntegration.Shared.Observability;

namespace PowrIntegration.BackOfficeService;

internal sealed class Worker(
    IOptions<BackOfficeServiceOptions> serviceOptions,
    BackOfficeServiceRabbitMqFactory messageQueueFactory,
    PluItemsFileImport pluItemsImport,
    ClassificationCodesFileImport classificationImport,
    Outbox outbox,
    IMetrics metrics,
    ILogger<Worker> logger) : BackgroundService
{
    private readonly BackOfficeServiceOptions _serviceOptions = serviceOptions.Value;
    private readonly BackOfficeServiceRabbitMqFactory _messageQueueFactory = messageQueueFactory;
    private readonly Outbox _outbox = outbox;
    private readonly IMetrics _metrics = metrics;
    private readonly PluItemsFileImport _pluItemsFileImport = pluItemsImport;
    private readonly ClassificationCodesFileImport _classificationFileImport = classificationImport;
    private readonly ILogger<Worker> _logger = logger;

    protected override async Task ExecuteAsync(CancellationToken cancellationToken)
    {
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

                await _classificationFileImport.Execute(cancellationToken);

                await _pluItemsFileImport.Execute(cancellationToken);

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
