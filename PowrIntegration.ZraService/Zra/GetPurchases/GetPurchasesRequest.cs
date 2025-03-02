namespace PowrIntegration.ZraService.Zra.GetPurchases;

public sealed record GetPurchasesRequest : ZraRequest
{
    public string lastReqDt { get; init; } = "20160523000000";
}
