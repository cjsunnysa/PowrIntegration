using PowrIntegration.ZraService.Zra;

namespace PowrIntegration.ZraService.Zra.StandardCodes;

public sealed record FetchStandardCodesRequest : ZraRequest
{
    public string lastReqDt { get; init; } = "20231215000000";
}
