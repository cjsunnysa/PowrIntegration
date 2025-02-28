namespace PowrIntegrationService.Data.Entities;

public sealed record ZraImportItem
{
    public string DeclarationNumber { get; init; } = string.Empty; // Declaration number
    public int ItemSequenceNumber { get; init; } // Item sequence in declaration
    public string TaskCode { get; set; } = string.Empty; // Task code related to customs clearance
    public DateTime? DeclarationDate { get; set; } // Declaration date
    public string? HarmonizedSystemCode { get; set; } = string.Empty; // Harmonized System code
    public string ItemName { get; set; } = string.Empty; // Product name
    public string? OriginCountryCode { get; set; } // Origin nation code
    public string? ExportCountryCode { get; set; } // Export nation code
    public decimal PackageQuantity { get; set; } // Package quantity
    public string? PackageUnitCode { get; set; } // Package unit code
    public decimal Quantity { get; set; } // Quantity of the imported item
    public string? QuantityUnitCode { get; set; } // Quantity unit code
    public decimal? TotalWeight { get; set; } // Total weight
    public decimal? NetWeight { get; set; } // Net weight
    public string SupplierName { get; set; } = string.Empty; // Supplier name
    public string? AgentName { get; set; } // Agent name
    public decimal InvoiceForeignCurrencyAmount { get; set; } // Invoice foreign currency amount
    public string InvoiceForeignCurrencyCode { get; set; } = string.Empty; // Invoice foreign currency code
    public decimal InvoiceForeignCurrencyExchangeRate { get; set; } // Invoice foreign currency exchange rate
    public string? DeclarationReferenceNumber { get; set; } // Declaration reference number
}
