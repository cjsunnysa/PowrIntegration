namespace PowrIntegration.Dtos;
public sealed record ImportItemDto
{
    public required string TaskCode { get; init; } // Task code related to customs clearance
    public required int ItemSequenceNumber { get; init; } // Item sequence in declaration
    public required string DeclarationNumber { get; init; } // Declaration number
    public string? DeclarationReferenceNumber { get; init; } = string.Empty; // Declaration reference number
    public DateTime? DeclarationDate { get; init; } // Declaration date
    public string? HarmonizedSystemCode { get; init; } // Harmonized System code
    public required string ItemName { get; init; } // Product name
    public string? OriginCountryCode { get; init; } // Origin nation code
    public string? ExportCountryCode { get; init; } // Export nation code
    public required decimal PackageQuantity { get; init; } // Package quantity
    public string? PackageUnitCode { get; init; } // Package unit code
    public required decimal Quantity { get; init; } // Quantity of the imported item
    public string? QuantityUnitCode { get; init; } // Quantity unit code
    public decimal? TotalWeight { get; init; } // Total weight
    public decimal? NetWeight { get; init; } // Net weight
    public required string SupplierName { get; init; } // Supplier name
    public string? AgentName { get; init; } // Agent name
    public required decimal InvoiceForeignCurrencyAmount { get; init; } // Invoice foreign currency amount
    public required string InvoiceForeignCurrencyCode { get; init; } // Invoice foreign currency code
    public required decimal InvoiceForeignCurrencyExchangeRate { get; init; } // Invoice foreign currency exchange rate
}
