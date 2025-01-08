using FluentResults;
using Microsoft.Extensions.Options;
using PowrIntegration.Data.Entities;
using PowrIntegration.Options;

namespace PowrIntegration.MessageQueue;

public sealed class ZraQueuePublisher(
    IOptions<PowertillOptions> powertillOptions,
    IOptions<ApiOptions> apiOptions,
    RabbitMqFactory factory,
    ILogger<ZraQueuePublisher> logger)
{
    private readonly PowertillOptions _powertillOptions = powertillOptions.Value;
    private readonly ApiOptions _apiOptions = apiOptions.Value;
    private readonly RabbitMqFactory _factory = factory;
    private readonly ILogger<ZraQueuePublisher> _logger = logger;

    public async Task<Result> PublishOutboxItem(OutboxItem record, CancellationToken cancellationToken)
    {
        try
        {
            RabbitMqPublisher queuePublisher = await CreatePublisher(cancellationToken);
            
            await queuePublisher.Publish(record.MessageType, record.MessageBody, cancellationToken);

            return Result.Ok();
        }
        catch (Exception ex)
        {
            return Result.Fail(new ExceptionalError("An exception occured pushing outbox record to the queue.", ex));
        }
    }

    private async Task<RabbitMqPublisher> CreatePublisher(CancellationToken cancellationToken) => await _factory.CreatePublisher(_apiOptions.QueueHost, _apiOptions.QueueName, cancellationToken);
}
