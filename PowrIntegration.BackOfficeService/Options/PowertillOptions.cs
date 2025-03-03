namespace PowrIntegration.BackOfficeService.Options;

public sealed record PowertillOptions
{
    public const string KEY = "Powertill";

    public required string FileOutputDirectory { get; init; }
    public required string FileOutputDirectoryWindowsPath { get; init; }
}