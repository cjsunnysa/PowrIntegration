using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Options;
using PowrIntegration.Data;
using PowrIntegration.Data.Exporters;
using PowrIntegration.Data.Importers;
using PowrIntegration.Dtos;
using PowrIntegration.Extensions;
using PowrIntegration.MessageQueue;
using PowrIntegration.Options;
using PowrIntegration.Zra;
using System.Collections.Immutable;

namespace PowrIntegration;

public class Worker : BackgroundService
{
    private readonly ServicesOptions _servicesOptions;
    private readonly IDbContextFactory<PowrIntegrationDbContext> _dbContextFactory;
    private readonly PowertillQueuePublisher _powertillQueuePublisher;
    private readonly PowertillQueueConsumer _powertillQueueConsumer;
    private readonly ZraQueueConsumer _zraQueueConsumer;
    private readonly Outbox _outbox;
    private readonly ZraService _zraService;
    private readonly PluItemsImport _pluItemsImport;
    private readonly ClassificationCodesImport _classificationImport;
    private readonly ILogger<Worker> _logger;

    public Worker(
        IOptions<ServicesOptions> servicesOptions,
        IDbContextFactory<PowrIntegrationDbContext> dbContextFactory,
        PowertillQueuePublisher powertillQueuePublisher,
        PowertillQueueConsumer powertillQueueConsumer,
        ZraQueueConsumer zraQueueConsumer,
        Outbox outbox,
        ZraService zraService,
        PluItemsImport pluItemsImport,
        ClassificationCodesImport classificationImport,
        ILogger<Worker> logger)
    {
        _servicesOptions = servicesOptions.Value;
        _pluItemsImport = pluItemsImport;
        _classificationImport = classificationImport;
        _dbContextFactory = dbContextFactory;
        _powertillQueuePublisher = powertillQueuePublisher;
        _powertillQueueConsumer = powertillQueueConsumer;
        _zraQueueConsumer = zraQueueConsumer;
        _outbox = outbox;
        _zraService = zraService;
        _logger = logger;
    }

    protected override async Task ExecuteAsync(CancellationToken cancellationToken)
    {
        await _classificationImport.Execute(cancellationToken);
        await _pluItemsImport.Execute(cancellationToken);

        var fetchStandardCodesResult = await _zraService.FetchStandardCodes(cancellationToken);

        fetchStandardCodesResult.LogErrors(_logger);

        if (fetchStandardCodesResult.IsSuccess)
        {
            ImmutableArray<StandardCodeClassDto> standardCodeClasses = fetchStandardCodesResult.Value;

            var publishResult = await _powertillQueuePublisher.PublishStandardCodes(standardCodeClasses, cancellationToken);

            publishResult.LogErrors(_logger);
        }

        var fetchClassificationCodesResult = await _zraService.FetchClassificationCodes(cancellationToken);

        fetchClassificationCodesResult.LogErrors(_logger);

        if (fetchClassificationCodesResult.IsSuccess)
        {
            ImmutableArray<ClassificationCodeDto> classificationCodes = fetchClassificationCodesResult.Value;

            var publishResult = await _powertillQueuePublisher.PublishClassificationCodes(classificationCodes, cancellationToken);

            publishResult.LogErrors(_logger);
        }

        var fetchImports = await _zraService.GetImports(cancellationToken);

        fetchImports.LogErrors(_logger);

        if (fetchImports.IsSuccess)
        {
            ImmutableArray<ImportItemDto> importItems = fetchImports.Value;

            var publishResult = await _powertillQueuePublisher.PublishZraImportItems(importItems, cancellationToken);

            publishResult.LogErrors(_logger);
        }

        var zraConsumerStartResult = await _zraQueueConsumer.Start(cancellationToken);

        zraConsumerStartResult.LogErrors(_logger);

        var powertillConsumerStartResult = await _powertillQueueConsumer.Start(cancellationToken);

        powertillConsumerStartResult.LogErrors(_logger);

        int serviceTimeoutMilliseconds = Convert.ToInt32(TimeSpan.FromSeconds(_servicesOptions.ServiceTimeoutSeconds).TotalMilliseconds);

        while (!cancellationToken.IsCancellationRequested)
        {
            try
            {
                if (_logger.IsEnabled(LogLevel.Information))
                {
                    _logger.LogInformation("PowrIntegration worker running at: {time}", DateTimeOffset.Now);
                }

                await _outbox.SendItemsToApiQueue(cancellationToken);

                await Task.Delay(serviceTimeoutMilliseconds, cancellationToken);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "An exception occurred in the main service loop.");
            }
        }
    }
}
