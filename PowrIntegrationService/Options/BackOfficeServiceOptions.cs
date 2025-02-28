namespace PowrIntegrationService.Options;

public sealed record BackOfficeServiceOptions
{
    public required int ServiceTimeoutSeconds { get; init; }
    public required string ImportDirectory { get; init; }
}
