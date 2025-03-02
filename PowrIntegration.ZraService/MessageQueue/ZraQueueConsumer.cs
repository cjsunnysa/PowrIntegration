using FluentResults;
using System.Text.Json;
using RabbitMQ.Client;
using PowrIntegration.Shared.Extensions;
using PowrIntegration.Shared.Options;
using PowrIntegration.Shared.Dtos;
using PowrIntegration.ZraService.Errors;
using PowrIntegration.Shared.MessageQueue;
using PowrIntegration.ZraService.Zra;
using PowrIntegration.Shared.Observability;

namespace PowrIntegration.ZraService.MessageQueue;

public sealed class ZraQueueConsumer(
    IChannel channel,
    MessageQueueOptions options,
    ZraRestService zraService,
    IMetrics metrics,
    ILogger<ZraQueueConsumer> logger) : RabbitMqConsumer(channel, options, metrics.MetricsMeterName, logger)
{
    private readonly ZraRestService _zraService = zraService;
    private readonly ILogger<ZraQueueConsumer> _logger = logger;

    protected async override Task<MessageAction> HandleMessage(QueueMessageType messageType, ReadOnlyMemory<byte> body, CancellationToken cancellationToken)
    {
        var result = messageType switch
        {
            QueueMessageType.ItemInsert => await HandleItemInsertMessage(body, cancellationToken),
            QueueMessageType.ItemUpdate => await HandleItemUpdateMessage(body, cancellationToken),
            QueueMessageType.SavePurchase => await HandleSavePurchaseMessage(body, cancellationToken),
            _ => Result.Fail($"Unkown message type: {Enum.GetName(messageType)}.")
        };

        result.LogErrors(_logger);

        if (result.IsFailed && result.HasError<CircuitBreakerError>())
        {
            _logger.LogInformation("Pausing queue: {QueueName} processing due to a circuit breaker being open.", Options.Name);

            Thread.Sleep((int)TimeSpan.FromMinutes(1).TotalMilliseconds);

            return MessageAction.Requeue;
        }

        if (result.IsFailed && result.HasError<HttpRequestTimoutError>())
        {
            return MessageAction.Requeue;
        }

        return
            result.IsSuccess
            ? MessageAction.Acknowledge
            : MessageAction.Reject;
    }

    private async Task<Result> HandleItemInsertMessage(ReadOnlyMemory<byte> body, CancellationToken cancellationToken)
    {
        using var stream = new MemoryStream(body.ToArray());

        var plu = await JsonSerializer.DeserializeAsync<PluItemDto>(stream, cancellationToken: cancellationToken);

        return plu is null
            ? Result.Fail("Save plu item message processing error. Message body did not contain a valid PLU record.")
            : await _zraService.SaveItem(plu, cancellationToken);
    }

    private async Task<Result> HandleItemUpdateMessage(ReadOnlyMemory<byte> body, CancellationToken cancellationToken)
    {
        using var stream = new MemoryStream(body.ToArray());

        var plu = await JsonSerializer.DeserializeAsync<PluItemDto>(stream, cancellationToken: cancellationToken);

        return plu is null
            ? Result.Fail("Update plu item message processing error. Message body did not contain a valid PLU record.")
            : await _zraService.UpdateItem(plu, cancellationToken);
    }

    private async Task<Result> HandleSavePurchaseMessage(ReadOnlyMemory<byte> body, CancellationToken cancellationToken)
    {
        using var stream = new MemoryStream(body.ToArray());

        var purchase = await JsonSerializer.DeserializeAsync<PurchaseDto>(stream, cancellationToken: cancellationToken);

        return purchase is null
            ? Result.Fail("Save purchase message processing error. Message body did not contain a valid purchase record.")
            : await _zraService.SavePurchase(purchase, cancellationToken);
    }
}
