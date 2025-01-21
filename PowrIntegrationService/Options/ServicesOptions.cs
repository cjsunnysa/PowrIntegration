namespace PowrIntegrationService.Options;

public sealed class ServicesOptions
{
    public const string KEY = "Services";

    public int ServiceTimeoutSeconds { get; init; }
}
