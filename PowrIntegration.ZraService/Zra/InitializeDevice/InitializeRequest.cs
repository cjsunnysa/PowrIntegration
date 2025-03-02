using PowrIntegration.ZraService.Zra;

namespace PowrIntegration.ZraService.Zra.InitializeDevice;

public sealed record InitializeRequest : ZraRequest
{
    public required string dvcSrlNo { get; init; }
}
