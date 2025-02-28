
using PowrIntegration.Shared.Options;

namespace PowrIntegrationService.Options;

public sealed record RabbitMqOptions
{
    public const string KEY = "RabbitMq";

    public required int ConnectionRetryTimeoutSeconds { get; init; }
    public required MessageQueueOptions BackOfficeQueue { get; init; }
    public required MessageQueueOptions ApiQueue { get; init; }
}