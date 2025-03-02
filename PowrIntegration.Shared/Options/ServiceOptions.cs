namespace PowrIntegration.Shared.Options;

public record ServiceOptions
{
    public const string KEY = "Service";

    public required int TimeoutSeconds { get; init; }
}
