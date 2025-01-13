using CsvHelper;
using FluentResults;
using Microsoft.EntityFrameworkCore.Internal;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Options;
using PowrIntegration.Data;
using PowrIntegration.Options;
using RabbitMQ.Client;
using RabbitMQ.Client.Events;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using static PowrIntegration.MessageQueue.RabbitMqConsumer;
using PowrIntegration.Extensions;

namespace PowrIntegration.MessageQueue;
public sealed class SalesQueueConsumer(
    IOptions<PowertillOptions> options, 
    RabbitMqFactory factory, 
    IDbContextFactory<PowrIntegrationDbContext> dbContextFactory, 
    ILogger<SalesQueueConsumer> logger) 
{
    private readonly PowertillOptions _options = options.Value;
    private readonly RabbitMqFactory _factory = factory;
    private readonly IDbContextFactory<PowrIntegrationDbContext> _dbContextFactory = dbContextFactory;
    private readonly ILogger<SalesQueueConsumer> _logger = logger;
    private RabbitMqConsumer? _consumer;

    public async Task<Result> Start(CancellationToken cancellationToken)
    {
        try
        {
            _consumer = await _factory.CreateConsumer(_options.QueueHost, _options.QueueName, HandleMessage, cancellationToken);

            await _consumer.Start(cancellationToken);

            return Result.Ok();
        }
        catch (Exception ex)
        {
            return Result.Fail(new ExceptionalError(ex));
        }
    }

    private async Task<MessageAction> HandleMessage(BasicDeliverEventArgs args, CancellationToken cancellationToken)
    {
        try
        {
            Result<QueueMessageType> messageTypeResult = args.GetQueueMessageType();

            if (messageTypeResult.IsFailed)
            {
                messageTypeResult.LogErrors(_logger);

                return MessageAction.Reject;
            }

            var messageType = messageTypeResult.Value;

            var result = messageType switch
            {
                QueueMessageType.Sale => await HandleSaleMessage(args.Body, cancellationToken),
                _ => Result.Fail($"Unkown message type: {(int)messageType}.")
            };

            result.LogErrors(_logger);

            return
                result.IsSuccess
                ? MessageAction.Acknowledge
                : MessageAction.Reject;
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "An error occurred processing message: {MessageId} from queue: {QueueName}.", args.BasicProperties.MessageId, _options.QueueName);

            return MessageAction.Reject;
        }
    }

    private async Task<Result> HandleSaleMessage(ReadOnlyMemory<byte> body, CancellationToken cancellationToken)
    {
        return await Task.FromResult(Result.Ok());
    }
}
