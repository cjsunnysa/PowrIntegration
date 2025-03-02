namespace PowrIntegration.ZraService.Zra.ClassificationCodes;

public sealed record FetchClassificationCodesRequest : ZraRequest
{
    public string lastReqDt { get; init; } = "20231215000000";
}
