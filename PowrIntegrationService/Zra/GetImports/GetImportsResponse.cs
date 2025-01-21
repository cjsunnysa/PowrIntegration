using PowrIntegrationService.Zra;
using System.Collections.Immutable;

namespace PowrIntegrationService.Zra.GetImports;

public sealed record GetImportsResponse : ZraResponse
{
    public ImmutableArray<ImportItem>? data { get; init; } // List of import items

    public record ImportItem
    {
        public string? taskCd { get; init; } // Task code related to customs clearance
        public string? dclDe { get; init; } // Declaration date
        public required int itemSeq { get; init; } // Item sequence in declaration
        public required string dclNo { get; init; } // Declaration number
        public string? hsCd { get; init; } // Harmonized System code
        public string? itemNm { get; init; } // Product name
        public string? orgnNatCd { get; init; } // Origin nation code
        public string? exptNatCd { get; init; } // Export nation code
        public decimal? pkg { get; init; } // Package quantity
        public string? pkgUnitCd { get; init; } // Package unit code
        public decimal? qty { get; init; } // Quantity of the imported item
        public string? qtyUnitCd { get; init; } // Quantity unit code
        public decimal? totWt { get; init; } // Total weight
        public decimal? netWt { get; init; } // Net weight
        public string? spplrNm { get; init; } // Supplier name
        public string? agntNm { get; init; } // Agent name
        public decimal? invcFcurAmt { get; init; } // Invoice foreign currency amount
        public string? invcFcurCd { get; init; } // Invoice foreign currency code
        public decimal invcFcurExcrt { get; init; } // Invoice foreign currency exchange rate
        public string? dclRefNum { get; init; } // Declaration reference number
    }
}
