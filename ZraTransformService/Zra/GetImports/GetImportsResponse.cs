using System.Collections.Immutable;

namespace PowrIntegration.Zra.GetImports;

public sealed record GetImportsResponse : ZraResponse
{
    public ImmutableArray<ImportItem> data { get; init; } = new(); // List of import items

    public record ImportItem
    {
        public string taskCd { get; init; } = string.Empty; // Task code related to customs clearance
        public string dclDe { get; init; } = string.Empty; // Declaration date
        public int itemSeq { get; init; } // Item sequence in declaration
        public string dclNo { get; init; } = string.Empty; // Declaration number
        public string hsCd { get; init; } = string.Empty; // Harmonized System code
        public string itemNm { get; init; } = string.Empty; // Product name
        public string orgnNatCd { get; init; } = string.Empty; // Origin nation code
        public string exptNatCd { get; init; } = string.Empty; // Export nation code
        public decimal pkg { get; init; } // Package quantity
        public string pkgUnitCd { get; init; } = string.Empty; // Package unit code
        public decimal qty { get; init; } // Quantity of the imported item
        public string qtyUnitCd { get; init; } = string.Empty; // Quantity unit code
        public decimal totWt { get; init; } // Total weight
        public decimal netWt { get; init; } // Net weight
        public string spplrNm { get; init; } = string.Empty; // Supplier name
        public string agntNm { get; init; } = string.Empty; // Agent name
        public decimal invcFcurAmt { get; init; } // Invoice foreign currency amount
        public string invcFcurCd { get; init; } = string.Empty; // Invoice foreign currency code
        public decimal invcFcurExcrt { get; init; } // Invoice foreign currency exchange rate
        public string dclRefNum { get; init; } = string.Empty; // Declaration reference number
    }
}
