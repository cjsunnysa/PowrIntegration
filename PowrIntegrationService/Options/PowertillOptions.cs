namespace PowrIntegrationService.Options;

public sealed record PowertillOptions
{
    public const string KEY = "Services:Powertill";

    public string QueueName { get; set; } = string.Empty;
    public string QueueHost { get; set; } = string.Empty;
    public string ImportDirectory { get; init; } = string.Empty;
}