using PowrIntegration.Shared.Options;

namespace PowrIntegration.BackOfficeService.Options;

public sealed record BackOfficeServiceOptions : ServiceOptions
{
    public required string ImportDirectory { get; init; }
}
