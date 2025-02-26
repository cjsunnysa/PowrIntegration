using System.Collections.Immutable;

namespace PowrIntegrationService.Zra.SavePurchase;

public sealed record SavePurchaseRequest : ZraRequest
{
    public sealed record PurchaseItem
    {
        public decimal? ItemSeq { get; init; }
        public required string ItemCd { get; init; }
        public string? ItemClsCd { get; init; }
        public string? ItemNm { get; init; }
        public string? Bcd { get; init; }
        public string? SpplrItemClsCd { get; init; }
        public string? SpplrItemCd { get; init; }
        public string? SpplrItemNm { get; init; }
        public string? PkgUnitCd { get; init; }
        public decimal? Pkg { get; init; }
        public required string QtyUnitCd { get; init; }
        public required decimal Qty { get; init; }
        public required decimal Prc { get; init; }
        public required decimal SplyAmt { get; init; }
        public required decimal DcRt { get; init; }
        public required decimal DcAmt { get; init; }
        public string? TaxTyCd { get; init; }
        public string? IplCatCd { get; init; }
        public string? TlCatCd { get; init; }
        public string? ExciseCatCd { get; init; }
        public decimal? TaxblAmt { get; init; }
        public string? VatCatCd { get; init; }
        public decimal? IplTaxblAmt { get; init; }
        public decimal? TlTaxblAmt { get; init; }
        public decimal? ExciseTaxblAmt { get; init; }
        public decimal? TaxAmt { get; init; }
        public decimal? IplAmt { get; init; }
        public decimal? TlAmt { get; init; }
        public decimal? ExciseTxAmt { get; init; }
        public required decimal TotAmt { get; init; }
    }

    public required string InvcNo { get; init; }
    public decimal? OrgInvcNo { get; init; }
    public string? SpplrTpin { get; init; }
    public string? SpplrBhfId { get; init; }
    public string? SpplrNm { get; init; }
    public decimal? SpplrInvcNo { get; init; }
    public required string RegTyCd { get; init; }
    public required string PchsTyCd { get; init; }
    public required string RcptTyCd { get; init; }
    public required string PmtTyCd { get; init; }
    public required string PchsSttsCd { get; init; }
    public required string CfmDt { get; init; }
    public required string PchsDt { get; init; }
    public string? CnclReqDt { get; init; }
    public string? CnclDt { get; init; }
    public decimal? TotItemCnt { get; init; }
    public decimal? TotTaxblAmt { get; init; }
    public decimal? TotTaxAmt { get; init; }
    public decimal? TotAmt { get; init; }
    public string? Remark { get; init; }
    public required string RegrNm { get; init; }
    public required string RegrId { get; init; }
    public required string ModrNm { get; init; }
    public required string ModrId { get; init; }

    public required ImmutableArray<PurchaseItem> ItemList { get; init; } = [];
}