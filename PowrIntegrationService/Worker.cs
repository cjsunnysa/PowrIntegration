using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Conventions;
using Microsoft.Extensions.Options;
using PowrIntegrationService.Data;
using PowrIntegrationService.Data.Exporters;
using PowrIntegrationService.Data.Importers;
using PowrIntegrationService.Dtos;
using PowrIntegrationService.Extensions;
using PowrIntegrationService.MessageQueue;
using PowrIntegrationService.Options;
using PowrIntegrationService.Powertill;
using PowrIntegrationService.Zra;
using System.Collections.Immutable;

namespace PowrIntegrationService;

public class Worker : BackgroundService
{
    private readonly IntegrationServiceOptions _serviceOptions;
    private readonly IDbContextFactory<PowrIntegrationDbContext> _dbContextFactory;
    private readonly RabbitMqFactory _messageQueueFactory;
    private readonly Outbox _outbox;
    private readonly ZraService _zraService;
    private readonly PluItemsFileImport _pluItemsFileImport;
    private readonly ClassificationCodesFileImport _classificationFileImport;
    private readonly ILogger<Worker> _logger;

    public Worker(
        IOptions<IntegrationServiceOptions> serviceOptions,
        IDbContextFactory<PowrIntegrationDbContext> dbContextFactory,
        RabbitMqFactory messageQueueFactory,
        ZraService zraService,
        PluItemsFileImport pluItemsImport,
        ClassificationCodesFileImport classificationImport,
        Outbox outbox,
        ILogger<Worker> logger)
    {
        _serviceOptions = serviceOptions.Value;
        _pluItemsFileImport = pluItemsImport;
        _classificationFileImport = classificationImport;
        _dbContextFactory = dbContextFactory;
        _messageQueueFactory = messageQueueFactory;
        _zraService = zraService;
        _outbox = outbox;
        _logger = logger;
    }

    protected override async Task ExecuteAsync(CancellationToken cancellationToken)
    {
        var backOfficeQueuePublisher = await _messageQueueFactory.CreatePublisher<BackOfficeQueuePublisher>(cancellationToken);

        await _classificationFileImport.Execute(cancellationToken);

        await _pluItemsFileImport.Execute(cancellationToken);

        await FetchStandardCodes(backOfficeQueuePublisher, cancellationToken);

        await FetchClassificationCodes(backOfficeQueuePublisher, cancellationToken);

        var zraQueueConsumer = await _messageQueueFactory.CreateConsumer<ZraQueueConsumer>(cancellationToken);

        await zraQueueConsumer.Start(cancellationToken);

        var backOfficeQueueConsumer = await _messageQueueFactory.CreateConsumer<BackOfficeQueueConsumer>(cancellationToken);

        await backOfficeQueueConsumer.Start(cancellationToken);

        int serviceTimeoutMilliseconds = Convert.ToInt32(TimeSpan.FromSeconds(_serviceOptions.ServiceTimeoutSeconds).TotalMilliseconds);

        while (!cancellationToken.IsCancellationRequested)
        {
            try
            {
                if (_logger.IsEnabled(LogLevel.Information))
                {
                    _logger.LogInformation("PowrIntegration worker running at: {time}", DateTimeOffset.Now);
                }

                await PublishOutboxItems(cancellationToken);

                await FetchImports(backOfficeQueuePublisher, cancellationToken);
                
                await FetchPurchases(backOfficeQueuePublisher, cancellationToken);

                await Task.Delay(serviceTimeoutMilliseconds, cancellationToken);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "An exception occurred in the main service loop.");
            }
        }
    }

    private async Task FetchPurchases(BackOfficeQueuePublisher queuePublisher, CancellationToken cancellationToken)
    {
        var fetchPurchases = await _zraService.GetPurchases(cancellationToken);

        fetchPurchases.LogErrors(_logger);

        if (fetchPurchases.IsSuccess)
        {
            ImmutableArray<PurchaseDto> purchases = fetchPurchases.Value;

            var publishResult = await queuePublisher.PublishPurchases(purchases, cancellationToken);

            publishResult.LogErrors(_logger);
        }
    }

    private async Task FetchImports(BackOfficeQueuePublisher queuePublisher, CancellationToken cancellationToken)
    {
        var fetchImports = await _zraService.GetImports(cancellationToken);

        fetchImports.LogErrors(_logger);

        if (fetchImports.IsSuccess)
        {
            ImmutableArray<ImportItemDto> importItems = fetchImports.Value;

            var publishResult = await queuePublisher.PublishZraImportItems(importItems, cancellationToken);

            publishResult.LogErrors(_logger);
        }
    }

    private async Task FetchClassificationCodes(BackOfficeQueuePublisher queuePublisher, CancellationToken cancellationToken)
    {
        var fetchClassificationCodesResult = await _zraService.FetchClassificationCodes(cancellationToken);

        fetchClassificationCodesResult.LogErrors(_logger);

        if (fetchClassificationCodesResult.IsSuccess)
        {
            ImmutableArray<ClassificationCodeDto> classificationCodes = fetchClassificationCodesResult.Value;

            var publishResult = await queuePublisher.PublishClassificationCodes(classificationCodes, cancellationToken);

            publishResult.LogErrors(_logger);
        }
    }

    private async Task FetchStandardCodes(BackOfficeQueuePublisher queuePublisher, CancellationToken cancellationToken)
    {
        var fetchStandardCodesResult = await _zraService.FetchStandardCodes(cancellationToken);

        fetchStandardCodesResult.LogErrors(_logger);

        if (fetchStandardCodesResult.IsSuccess)
        {
            ImmutableArray<StandardCodeClassDto> standardCodeClasses = fetchStandardCodesResult.Value;

            var publishResult = await queuePublisher.PublishStandardCodes(standardCodeClasses, cancellationToken);

            publishResult.LogErrors(_logger);
        }
    }

    private async Task PublishOutboxItems(CancellationToken cancellationToken)
    {
        await _outbox.PublishToQueue(cancellationToken);
    }
}
