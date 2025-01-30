using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Options;
using PowrIntegrationService.Data;
using PowrIntegrationService.Data.Exporters;
using PowrIntegrationService.Data.Importers;
using PowrIntegrationService.Dtos;
using PowrIntegrationService.Extensions;
using PowrIntegrationService.MessageQueue;
using PowrIntegrationService.Options;
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
    private readonly PluItemsImport _pluItemsImport;
    private readonly ClassificationCodesImport _classificationImport;
    private readonly IngredientsImport _ingredientsImport;
    private readonly ILogger<Worker> _logger;

    public Worker(
        IOptions<IntegrationServiceOptions> serviceOptions,
        IDbContextFactory<PowrIntegrationDbContext> dbContextFactory,
        RabbitMqFactory messageQueueFactory,
        Outbox outbox,
        ZraService zraService,
        PluItemsImport pluItemsImport,
        ClassificationCodesImport classificationImport,
        IngredientsImport ingredientsImport,
        ILogger<Worker> logger)
    {
        _serviceOptions = serviceOptions.Value;
        _pluItemsImport = pluItemsImport;
        _classificationImport = classificationImport;
        _ingredientsImport = ingredientsImport;
        _dbContextFactory = dbContextFactory;
        _messageQueueFactory = messageQueueFactory;
        _outbox = outbox;
        _zraService = zraService;
        _logger = logger;
    }

    protected override async Task ExecuteAsync(CancellationToken cancellationToken)
    {
        var powertillQueuePublisher = await _messageQueueFactory.CreatePublisher<BackOfficeQueuePublisher>(cancellationToken);

        var fetchStandardCodesResult = await _zraService.FetchStandardCodes(cancellationToken);

        fetchStandardCodesResult.LogErrors(_logger);

        if (fetchStandardCodesResult.IsSuccess)
        {
            ImmutableArray<StandardCodeClassDto> standardCodeClasses = fetchStandardCodesResult.Value;

            var publishResult = await powertillQueuePublisher.PublishStandardCodes(standardCodeClasses, cancellationToken);

            publishResult.LogErrors(_logger);
        }

        var fetchClassificationCodesResult = await _zraService.FetchClassificationCodes(cancellationToken);

        fetchClassificationCodesResult.LogErrors(_logger);

        if (fetchClassificationCodesResult.IsSuccess)
        {
            ImmutableArray<ClassificationCodeDto> classificationCodes = fetchClassificationCodesResult.Value;

            var publishResult = await powertillQueuePublisher.PublishClassificationCodes(classificationCodes, cancellationToken);

            publishResult.LogErrors(_logger);
        }

        var fetchImports = await _zraService.GetImports(cancellationToken);

        fetchImports.LogErrors(_logger);

        if (fetchImports.IsSuccess)
        {
            ImmutableArray<ImportItemDto> importItems = fetchImports.Value;

            var publishResult = await powertillQueuePublisher.PublishZraImportItems(importItems, cancellationToken);

            publishResult.LogErrors(_logger);
        }

        var zraQueueConsumer = await _messageQueueFactory.CreateConsumer<ZraQueueConsumer>(cancellationToken);

        await zraQueueConsumer.Start(cancellationToken);

        var powertillQueueConsumer = await _messageQueueFactory.CreateConsumer<BackOfficeQueueConsumer>(cancellationToken);
        
        await powertillQueueConsumer.Start(cancellationToken);

        int serviceTimeoutMilliseconds = Convert.ToInt32(TimeSpan.FromSeconds(_serviceOptions.ServiceTimeoutSeconds).TotalMilliseconds);

        while (!cancellationToken.IsCancellationRequested)
        {
            try
            {
                if (_logger.IsEnabled(LogLevel.Information))
                {
                    _logger.LogInformation("PowrIntegration worker running at: {time}", DateTimeOffset.Now);
                }

                await _classificationImport.Execute(cancellationToken);
                await _pluItemsImport.Execute(cancellationToken);
                await _ingredientsImport.Execute(cancellationToken);
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
