using PowrIntegration.Shared.Options;

namespace PowrIntegration.PowertillService.Options;

public sealed record BackOfficeServiceOptions : ServiceOptions
{
    public required string ImportDirectory { get; init; }
}
