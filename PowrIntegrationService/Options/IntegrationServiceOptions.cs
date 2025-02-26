namespace PowrIntegrationService.Options;

public sealed record IntegrationServiceOptions
{
    public required int ServiceTimeoutSeconds { get; init; }
    public required string ImportDirectory { get; init; }
}
