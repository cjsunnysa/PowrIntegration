
namespace PowrIntegration.Shared.Options;

public sealed record RabbitMqOptions
{
    public const string KEY = "RabbitMq";

    public required int ConnectionRetryTimeoutSeconds { get; init; }
    public required MessageQueueOptions BackOfficeQueue { get; init; }
    public required MessageQueueOptions ApiQueue { get; init; }
}

public sealed record DeadLetterOptions
{
    public required string Name { get; init; }
    public required string ExchangeName { get; init; }
    public required string RoutingKey { get; init; }
}

public sealed record MessageQueueOptions
{
    public required string Name { get; init; }
    public required string Host { get; init; }
    public required DeadLetterOptions DeadLetterQueue { get; init; }
}

