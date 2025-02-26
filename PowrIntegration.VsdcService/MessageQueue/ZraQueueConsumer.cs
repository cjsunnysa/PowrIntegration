//using FluentResults;
//using System.Text.Json;
//using PowrIntegrationService.Options;
//using PowrIntegrationService.Errors;
//using PowrIntegrationService.Extensions;
//using PowrIntegrationService.Zra;
//using PowrIntegrationService.Dtos;
//using RabbitMQ.Client;

//namespace PowrIntegrationService.MessageQueue;

//public sealed class ZraQueueConsumer(
//    IChannel channel,
//    MessageQueueOptions options,
//    ZraService zraService,
//    ILogger<ZraQueueConsumer> logger) : RabbitMqConsumer(channel, options, logger)
//{
//    private readonly ZraService _zraService = zraService;
//    private readonly ILogger<ZraQueueConsumer> _logger = logger;

//    protected async override Task<MessageAction> HandleMessage(QueueMessageType messageType, ReadOnlyMemory<byte> body, CancellationToken cancellationToken)
//    {
//        var result = messageType switch
//        {
//            QueueMessageType.ItemInsert => await HandleItemInsertMessage(body, cancellationToken),
//            QueueMessageType.ItemUpdate => await HandleItemUpdateMessage(body, cancellationToken),
//            _ => Result.Fail($"Unkown message type: {Enum.GetName(messageType)}.")
//        };

//        result.LogErrors(_logger);

//        if (result.IsFailed && result.HasError<CircuitBreakerError>())
//        {
//            _logger.LogInformation("Pausing queue: {QueueName} processing due to a circuit breaker being open.", Options.Name);

//            Thread.Sleep((int)TimeSpan.FromMinutes(1).TotalMilliseconds);

//            return MessageAction.Requeue;
//        }

//        if (result.IsFailed && result.HasError<HttpRequestTimoutError>())
//        {
//            return MessageAction.Requeue;
//        }

//        return
//            result.IsSuccess
//            ? MessageAction.Acknowledge
//            : MessageAction.Reject;
//    }

//    private async Task<Result> HandleItemInsertMessage(ReadOnlyMemory<byte> body, CancellationToken cancellationToken)
//    {
//        using var stream = new MemoryStream(body.ToArray());

//        var plu = await JsonSerializer.DeserializeAsync<PluItemDto>(stream, cancellationToken: cancellationToken);

//        return plu is null
//            ? Result.Fail("Save plu item message processing error. Message body did not contain a valid PLU record.")
//            : await _zraService.SaveItem(plu, cancellationToken);
//    }

//    private async Task<Result> HandleItemUpdateMessage(ReadOnlyMemory<byte> body, CancellationToken cancellationToken)
//    {
//        using var stream = new MemoryStream(body.ToArray());

//        var plu = await JsonSerializer.DeserializeAsync<PluItemDto>(stream, cancellationToken: cancellationToken);

//        return plu is null
//            ? Result.Fail("Update plu item message processing error. Message body did not contain a valid PLU record.")
//            : await _zraService.UpdateItem(plu, cancellationToken);
//    }
//}
