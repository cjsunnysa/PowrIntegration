using PowrIntegration.ZraService.Zra;

namespace PowrIntegration.ZraService.Zra.SavePurchase;

public sealed record SavePurchasesResponse : ZraResponse
{
    public sealed record SavePurchasesResponseData
    {
        public required string cisInvcNo { get; init; }
    }

    public SavePurchasesResponseData? data { get; init; }
}