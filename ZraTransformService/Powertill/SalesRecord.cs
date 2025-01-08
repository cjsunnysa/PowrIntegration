namespace PowrIntegration.Powertill;

public sealed class SalesRecord
{
    public string RecordType { get; init; } = string.Empty;
    public int? RecordNumber { get; init; }
    public string? Description { get; init; }
    public string? Size { get; init; }
    public decimal Quantity { get; init; }
    public decimal Amount { get; init; }
    public decimal Tax1Amount { get; init; }
    public decimal Tax2Amount { get; init; }
    public decimal Tax3Amount { get; init; }
    public decimal Tax4Amount { get; init; }
    public decimal Tax5Amount { get; init; }
    public decimal Tax6Amount { get; init; }
    public decimal Discount1Amount { get; init; }
    public decimal Discount2Amount { get; init; }
    public decimal Discount3Amount { get; init; }
    public decimal Discount4Amount { get; init; }
    public decimal Discount5Amount { get; init; }
    public decimal Discount6Amount { get; init; }
    public decimal Discount7Amount { get; init; }
    public decimal Discount8Amount { get; init; }
    public decimal Discount9Amount { get; init; }
    public string? QuantityString { get; init; }
    public int? RecieptPrintGroup { get; init; }
    public int? ItemFlag1 { get; init; }
    public int? ItemFlag2 { get; init; }
    public string SaleDateTime { get; init; } = string.Empty;
    public string ClerkNumber { get; init; } = string.Empty;
    public string TerminalNumber { get; init; } = string.Empty;
    public string SequenceNumber { get; init; } = string.Empty;
    public string Version { get; init; } = string.Empty;

    public bool IsSalesItem => RecordType == "I";
    public bool IsFunction => RecordType == "F";
    public bool IsDiscount => RecordType == "R";
    public bool IsSubtotal => RecordType == "S";
    public bool IsTender => RecordType == "T";
    public bool IsTaxRate => RecordType == "X";
    public bool IsVatRate => RecordType == "Y";
    public bool IsEndOfSaleMarker => RecordType == "Z";
}
