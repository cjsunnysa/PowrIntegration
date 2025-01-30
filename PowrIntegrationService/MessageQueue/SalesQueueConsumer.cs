using FluentResults;
using Microsoft.EntityFrameworkCore;
using PowrIntegrationService.Data;
using PowrIntegrationService.Extensions;
using PowrIntegrationService.Options;
using RabbitMQ.Client;

namespace PowrIntegrationService.MessageQueue;
public sealed class SalesQueueConsumer(
    IChannel channel,
    MessageQueueOptions options,
    IDbContextFactory<PowrIntegrationDbContext> dbContextFactory,
    ILogger<SalesQueueConsumer> logger) : RabbitMqConsumer(channel, options, logger)
{
    private readonly IDbContextFactory<PowrIntegrationDbContext> _dbContextFactory = dbContextFactory;
    private readonly ILogger<SalesQueueConsumer> _logger = logger;

    protected override async Task<MessageAction> HandleMessage(QueueMessageType messageType, ReadOnlyMemory<byte> messageBody, CancellationToken cancellationToken)
    {
        var result = messageType switch
        {
            QueueMessageType.Sale => await HandleSaleMessage(messageBody, cancellationToken),
            _ => Result.Fail($"Unkown message type: {(int)messageType}.")
        };

        result.LogErrors(_logger);

        return
            result.IsSuccess
            ? MessageAction.Acknowledge
            : MessageAction.Reject;
    }

    private async Task<Result> HandleSaleMessage(ReadOnlyMemory<byte> body, CancellationToken cancellationToken)
    {
        return await Task.FromResult(Result.Ok());
    }
}
