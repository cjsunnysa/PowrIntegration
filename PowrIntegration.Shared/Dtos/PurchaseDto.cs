using System.Collections.Immutable;

namespace PowrIntegration.Shared.Dtos;

public sealed record PurchaseDto
{
    public required string SupplierTaxPayerIdentifier { get; init; } // VARCHAR(10)
    public required string SupplierName { get; init; } // VARCHAR(60)
    public string? SupplierBranchIdentifier { get; init; } // VARCHAR(3), Nullable
    public required long SupplierInvoiceNumber { get; init; } // NUMBER(38)
    public required string RecieptTypeCode { get; init; } // VARCHAR(5)
    public required string PaymentTypeCode { get; init; } // VARCHAR(5)
    public required string ConfirmedDate { get; init; } // VARCHAR(14), ISO 8601 datetime format
    public required string SalesDate { get; init; } // VARCHAR(8), ISO 8601 date format (yyyyMMdd)
    public string? StockReleaseDate { get; init; } // VARCHAR(14), Nullable, ISO 8601 datetime format
    public required int TotalItemCount { get; init; } // NUMBER(10)
    public required decimal TotalTaxableAmount { get; init; } // NUMBER(18,4)
    public required decimal TotalTaxAmount { get; init; } // NUMBER(18,2)
    public required decimal TotalAmount { get; init; } // NUMBER(18,2)
    public string? Remark { get; init; } // VARCHAR(400), Nullable
    public ImmutableArray<PurchaseItemDto> Items { get; init; } = []; // Nested List for Item Details

    public sealed record PurchaseItemDto
    {
        public required int SequenceNumber { get; init; } // NUMBER(4)
        public required string SupplierItemCode { get; init; }
        public required string ClassificationCode { get; init; }
        public required string Name { get; init; }
        public string? Barcode { get; init; }
        public string? PackagingUnitCode { get; init; }
        public decimal? Package { get; init; }
        public string? QuantityUnitCode { get; init; }
        public required decimal Quantity { get; init; }
        public required decimal InclusiveUnitPrice { get; init; }
        public decimal ExclusiveUnitPrice => TaxableAmount / Quantity;
        public required decimal SupplyAmount { get; init; }
        public required decimal DiscountRate { get; init; }
        public required decimal DiscountAmount { get; init; }
        public string? VatCategoryCode { get; init; }
        public string? IplCategoryCode { get; init; }
        public string? TlCategoryCode { get; init; }
        public string? ExciseCategoryCode { get; init; }
        public required decimal VatTaxableAmount { get; init; }
        public required decimal IplTaxableAmount { get; init; }
        public required decimal TlTaxableAmount { get; init; }
        public required decimal ExciseTaxableAmount { get; init; }
        public required decimal VatAmount { get; init; }
        public required decimal IplAmount { get; init; } // NUMBER(18,4)
        public required decimal TlAmount { get; init; }
        public required decimal ExciseAmount { get; init; }
        public decimal TaxAmount => VatAmount + IplAmount + TlAmount + ExciseAmount;
        public required decimal TaxableAmount { get; init; }
        public required decimal TotalAmount { get; init; } // NUMBER(18,4)
    }
}
