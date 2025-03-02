using FluentResults;
using Microsoft.Extensions.Options;
using PowrIntegration.Shared.Dtos;
using PowrIntegration.Shared.Extensions;
using PowrIntegration.Shared.File;
using PowrIntegration.Shared.Observability;
using PowrIntegration.Shared.Options;
using PowrIntegration.ZraService.MessageQueue;
using PowrIntegration.ZraService.Options;
using PowrIntegration.ZraService.Zra;
using PowrIntegration.ZraService.Zra.InitializeDevice;
using System.Collections.Immutable;

namespace PowrIntegration.ZraService;

internal class Worker(
    ZraServiceRabbitMqFactory messageQueueFactory,
    ZraRestService zraService,
    IOptions<ServiceOptions> serviceOptions,
    IOptions<ZraApiOptions> zraOptions,
    IMetrics metrics,
    ILogger<Worker> logger) : BackgroundService
{
    private readonly ZraServiceRabbitMqFactory _messageQueueFactory = messageQueueFactory;
    private readonly ZraRestService _zraService = zraService;
    private readonly ServiceOptions _serviceOptions = serviceOptions.Value;
    private readonly ZraApiOptions _zraOptions = zraOptions.Value;
    private readonly IMetrics _metrics = metrics;
    private readonly ILogger<Worker> _logger = logger;

    protected override async Task ExecuteAsync(CancellationToken cancellationToken)
    {
        var backOfficeQueuePublisher = await _messageQueueFactory.CreatePublisher(cancellationToken);

        var initializeDeviceResult = await InitializeDevice(cancellationToken);

        if (initializeDeviceResult.IsFailed)
        {
            return;
        }

        await FetchStandardCodes(backOfficeQueuePublisher, cancellationToken);

        await FetchClassificationCodes(backOfficeQueuePublisher, cancellationToken);

        var zraQueueConsumer = await _messageQueueFactory.CreateConsumer(cancellationToken);

        await zraQueueConsumer.Start(cancellationToken);

        int serviceTimeoutMilliseconds = Convert.ToInt32(TimeSpan.FromSeconds(_serviceOptions.TimeoutSeconds).TotalMilliseconds);

        while (!cancellationToken.IsCancellationRequested)
        {
            try
            {
                if (_logger.IsEnabled(LogLevel.Information))
                {
                    _logger.LogInformation("{ApplicationName} service worker running at: {time}", _metrics.ApplicationName, DateTimeOffset.Now);
                }

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

    private async Task<Result> InitializeDevice(CancellationToken cancellationToken)
    {
        if (!_zraOptions.ShouldInitializeDevice)
        {
            return Result.Ok();
        }

        Result<InitializationResponse> result = await _zraService.InitializeDevice(cancellationToken);

        if (result.IsFailed)
        {
            result.LogErrors(_logger);

            return result.ToResult();
        }

        var initializeFile = new TextFile(_zraOptions.RegisterDeviceFileName);

        var writeFileResult = await initializeFile.Write(result.Value, cancellationToken);

        if (writeFileResult.IsFailed)
        {
            writeFileResult.LogErrors(_logger);
        }

        return writeFileResult;
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
}
