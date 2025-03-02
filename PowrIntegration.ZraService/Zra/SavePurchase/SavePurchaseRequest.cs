using PowrIntegration.ZraService.Zra;
using System.Collections.Immutable;

namespace PowrIntegration.ZraService.Zra.SavePurchase;

public sealed record SavePurchaseRequest : ZraRequest
{
    public sealed record PurchaseItem
    {
        public decimal? itemSeq { get; init; }
        public required string itemCd { get; init; }
        public string? itemClsCd { get; init; }
        public string? itemNm { get; init; }
        public string? bcd { get; init; }
        public string? spplrItemClsCd { get; init; }
        public string? spplrItemCd { get; init; }
        public string? spplrItemNm { get; init; }
        public string? pkgUnitCd { get; init; }
        public decimal? pkg { get; init; }
        public required string qtyUnitCd { get; init; }
        public required decimal qty { get; init; }
        public required decimal prc { get; init; }
        public required decimal splyAmt { get; init; }
        public required decimal dcRt { get; init; }
        public required decimal dcAmt { get; init; }
        public string? taxTyCd { get; init; }
        public string? iplCatCd { get; init; }
        public string? tlCatCd { get; init; }
        public string? exciseCatCd { get; init; }
        public decimal? taxblAmt { get; init; }
        public string? vatCatCd { get; init; }
        public decimal? iplTaxblAmt { get; init; }
        public decimal? tlTaxblAmt { get; init; }
        public decimal? exciseTaxblAmt { get; init; }
        public decimal? taxAmt { get; init; }
        public decimal? iplAmt { get; init; }
        public decimal? tlAmt { get; init; }
        public decimal? exciseTxAmt { get; init; }
        public required decimal totAmt { get; init; }
    }

    public required string invcNo { get; init; }
    public decimal? orgInvcNo { get; init; }
    public string? spplrTpin { get; init; }
    public string? spplrBhfId { get; init; }
    public string? spplrNm { get; init; }
    public decimal? spplrInvcNo { get; init; }
    public required string regTyCd { get; init; }
    public required string pchsTyCd { get; init; }
    public required string rcptTyCd { get; init; }
    public required string pmtTyCd { get; init; }
    public required string pchsSttsCd { get; init; }
    public required string cfmDt { get; init; }
    public required string pchsDt { get; init; }
    public string? cnclReqDt { get; init; }
    public string? cnclDt { get; init; }
    public decimal? totItemCnt { get; init; }
    public decimal? totTaxblAmt { get; init; }
    public decimal? totTaxAmt { get; init; }
    public decimal? totAmt { get; init; }
    public string? remark { get; init; }
    public required string regrNm { get; init; }
    public required string regrId { get; init; }
    public required string modrNm { get; init; }
    public required string modrId { get; init; }

    public required ImmutableArray<PurchaseItem> itemList { get; init; } = [];
}