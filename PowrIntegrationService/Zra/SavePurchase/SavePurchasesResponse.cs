namespace PowrIntegrationService.Zra.SavePurchase;

public sealed record SavePurchasesResponse : ZraResponse
{
    public sealed record SavePurchasesResponseData
    {
        public required string CisInvcNo { get; init; }
    }

    public SavePurchasesResponseData? Data { get; init; }
}