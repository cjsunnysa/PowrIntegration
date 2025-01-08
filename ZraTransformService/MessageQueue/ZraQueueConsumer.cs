using FluentResults;
using Microsoft.Extensions.Options;
using PowrIntegration.Options;
using RabbitMQ.Client.Events;
using PowrIntegration.Zra;
using System.Text.Json;
using PowrIntegration.Data.Entities;
using PowrIntegration.Dtos;

namespace PowrIntegration.MessageQueue;

public sealed class ZraQueueConsumer(
    IOptions<ApiOptions> apiOptions,
    RabbitMqFactory factory,
    ZraService zraService,
    ILogger<ZraQueueConsumer> logger)
{
    private readonly ApiOptions _apiOptions = apiOptions.Value;
    private readonly RabbitMqFactory _factory = factory;
    private readonly ZraService _zraService = zraService;
    private readonly ILogger<ZraQueueConsumer> _logger = logger;
    private RabbitMqConsumer? _queue;

    public async Task<Result> Start(CancellationToken cancellationToken)
    {
        try
        {
            _queue = await _factory.CreateConsumer(_apiOptions.QueueHost, _apiOptions.QueueName, HandleMessage, cancellationToken);

            await _queue.Start(cancellationToken);

            return Result.Ok();
        }
        catch (Exception ex)
        {
            return Result.Fail(new ExceptionalError(ex));
        }
    }

    private async Task<bool> HandleMessage(BasicDeliverEventArgs args, CancellationToken cancellationToken)
    {
        try
        { 
            Result<QueueMessageType> messageTypeResult = args.GetQueueMessageType();

            if (messageTypeResult.IsFailed)
            {
                messageTypeResult.LogErrors(_logger);
                
                return false;
            }

            var messageType = messageTypeResult.Value;

            var result = messageType switch
            {
                QueueMessageType.ItemInsert => await HandleItemInsertMessage(args.Body, cancellationToken),
                QueueMessageType.ItemUpdate => await HandleItemUpdateMessage(args.Body, cancellationToken),
                _ => Result.Fail($"Unkown message type: {Enum.GetName(messageType)}.")
            };

            result.LogErrors(_logger);

            return result.IsSuccess;
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "An exception occured processing a message from the {ApiQueueName} queue.", _apiOptions.QueueName);

            return false;
        }
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
}
