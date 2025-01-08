using FluentResults;
using RabbitMQ.Client.Events;
using System.Text.Json;
using ZraShared;
using ZraShared.MessageQueue;
using ZraShared.Zra;
using ZraSyncService.Zra;
using ZraSyncService.Zra.InitializeDevice;

namespace ZraSyncService;

public class Worker(
    ZraService zraService,
    IMessageQueueFactory queueFactory,
    ILogger<Worker> logger,
    IConfiguration configuration) : BackgroundService
{
    private readonly ZraService _zraService = zraService;
    private readonly IMessageQueueFactory _queueFactory = queueFactory;
    private readonly ILogger<Worker> _logger = logger;
    private readonly IConfiguration _configuration = configuration;
    private IMessageQueueConsumer? _syncQueue;
    private IMessageQueuePublisher? _transformQueue;

    protected override async Task ExecuteAsync(CancellationToken cancellationToken)
    {
        string transformQueueHost = _configuration.GetValue<string>("RabbitMQ:ZraTransformQueueHost")!;
        string transformQueueName = _configuration.GetValue<string>("RabbitMQ:ZraTransformQueueName")!;

        _transformQueue = await _queueFactory.CreatePublisher(transformQueueHost, transformQueueName, cancellationToken);

        string syncQueueHost = _configuration.GetValue<string>("RabbitMQ:ZraSyncQueueHost")!;
        string syncQueueName = _configuration.GetValue<string>("RabbitMQ:ZraSyncQueueName")!;

        _syncQueue = await _queueFactory.CreateConsumer(syncQueueHost, syncQueueName, HandleSyncQueueMessage, cancellationToken);

        bool shouldInitialize = _configuration.GetValue<bool>("ZraApi:ShouldInitializeDevice");

        if (shouldInitialize)
        {
            await InitializeDevice(cancellationToken);
        }

        await PublishStandardCodes(_transformQueue, cancellationToken);

        await PublishClassificationCodes(_transformQueue, cancellationToken);

        await _syncQueue.Start(cancellationToken);

        while (!cancellationToken.IsCancellationRequested)
        {
            if (_logger.IsEnabled(LogLevel.Information))
            {
                _logger.LogInformation("Worker running at: {time}", DateTimeOffset.Now);
            }

            var delay = _configuration.GetValue<int>("Sync:EverySeconds");

            await Task.Delay(delay * 1000, cancellationToken);
        }
    }

    private async Task<bool> HandleSyncQueueMessage(BasicDeliverEventArgs eventArgs, CancellationToken cancellationToken)
    {
        Result<QueueMessageType> messageTypeResult = eventArgs.GetQueueMessageType();

        if (messageTypeResult.IsFailed)
        {
            messageTypeResult.LogErrors(_logger);

            // add message to dead letter queue
        }

        var messageType = messageTypeResult.Value;

        var result = messageType switch
        {
            QueueMessageType.ItemInsert => await HandleItemInsertMessage(eventArgs.Body, cancellationToken),
            _ => Result.Fail($"Unkown message type: {Enum.GetName(messageType)}.")
        };

        result.LogErrors(_logger);

        return result.IsSuccess;
    }

    private async Task<Result> HandleItemInsertMessage(ReadOnlyMemory<byte> body, CancellationToken cancellationToken)
    {
        try
        {
            using var stream = new MemoryStream(body.ToArray());

            var deserializeResult = await JsonSerializer.DeserializeAsync<SaveItemRequest>(stream, cancellationToken: cancellationToken);

            return deserializeResult is null
                ? Result.Fail("Could not deserialize SaveItemRequest message from Sync queue.")
                : await _zraService.SaveItem(deserializeResult, cancellationToken);
        }
        catch (Exception ex)
        {
            return Result.Fail(new ExceptionalError(ex));
        }
    }

    private async Task InitializeDevice(CancellationToken cancellationToken)
    {
        _logger.LogInformation("Initializing device.");

        Result<InitializationResponse> initializeResult = await _zraService.InitializeDevice(cancellationToken);

        if (initializeResult.IsFailed)
        {
            initializeResult.LogErrors(_logger);

            return;
        }

        string initializeFilePath = _configuration.GetValue<string>("ZraApi:RegisterDeviceFilename")!;

        if (File.Exists(initializeFilePath))
        {
            return;
        }

        using var jsonStream = new MemoryStream();

        await JsonSerializer.SerializeAsync(jsonStream, initializeResult.Value, cancellationToken: cancellationToken);

        File.WriteAllBytes(initializeFilePath, jsonStream.ToArray());
    }

    private async Task PublishStandardCodes(IMessageQueuePublisher transformQueue, CancellationToken cancellationToken)
    {
        Result<FetchStandardCodesResponse> fetchStandardCodesResult = await _zraService.FetchStandardCodes(cancellationToken);

        if (fetchStandardCodesResult.IsFailed)
        {
            fetchStandardCodesResult.LogErrors(_logger);

            return;
        }

        if (fetchStandardCodesResult.Value.clsList.Length == 0)
        {
            _logger.LogInformation("Standard codes list is empty. Ignoring.");

            return;
        }

        using var memoryStream = new MemoryStream();

        await JsonSerializer.SerializeAsync(memoryStream, fetchStandardCodesResult.Value, cancellationToken: cancellationToken);

        var messageBytes = memoryStream?.ToArray() ?? [];

        await transformQueue.Publish(QueueMessageType.StandardCodes, messageBytes, cancellationToken);
    }

    private async Task PublishClassificationCodes(IMessageQueuePublisher transformQueue, CancellationToken cancellationToken)
    {
        Result<FetchClassificationCodesResponse> fetchClassificationCodesResult = await _zraService.FetchClassificationCodes(cancellationToken);

        if (fetchClassificationCodesResult.IsFailed)
        {
            fetchClassificationCodesResult.LogErrors(_logger);

            return;
        }

        if (fetchClassificationCodesResult.Value.itemClsList.Length == 0)
        {
            _logger.LogInformation("Classification codes list is empty or null. Ignoring.");

            return;
        }

        using var memoryStream = new MemoryStream();

        await JsonSerializer.SerializeAsync(memoryStream, fetchClassificationCodesResult.Value, cancellationToken: cancellationToken);
        
        var messageBytes = memoryStream?.ToArray() ?? [];

        await transformQueue.Publish(QueueMessageType.ClassificationCodes, messageBytes, cancellationToken);
    }
}
