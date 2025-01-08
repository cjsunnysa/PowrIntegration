namespace PowrIntegration.Data.Entities;

public sealed record ZraImportItem
{
    public string DeclarationNumber { get; init; } = string.Empty; // Declaration number
    public int ItemSequenceNumber { get; init; } // Item sequence in declaration
    public string TaskCode { get; init; } = string.Empty; // Task code related to customs clearance
    public DateTime? DeclarationDate { get; init; } // Declaration date
    public string? HarmonizedSystemCode { get; init; } = string.Empty; // Harmonized System code
    public string ItemName { get; init; } = string.Empty; // Product name
    public string? OriginCountryCode { get; init; } // Origin nation code
    public string? ExportCountryCode { get; init; } // Export nation code
    public decimal PackageQuantity { get; init; } // Package quantity
    public string? PackageUnitCode { get; init; } // Package unit code
    public decimal Quantity { get; init; } // Quantity of the imported item
    public string? QuantityUnitCode { get; init; } // Quantity unit code
    public decimal? TotalWeight { get; init; } // Total weight
    public decimal? NetWeight { get; init; } // Net weight
    public string SupplierName { get; init; } = string.Empty; // Supplier name
    public string? AgentName { get; init; } // Agent name
    public decimal InvoiceForeignCurrencyAmount { get; init; } // Invoice foreign currency amount
    public string InvoiceForeignCurrencyCode { get; init; } = string.Empty; // Invoice foreign currency code
    public decimal InvoiceForeignCurrencyExchangeRate { get; init; } // Invoice foreign currency exchange rate
    public string? DeclarationReferenceNumber { get; init; } // Declaration reference number
}
