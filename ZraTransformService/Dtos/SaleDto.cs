using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace PowrIntegration.Dtos;

public sealed record SaleDto
{
    public required char RecordType { get; init; }
    public required int Number { get; init; }
    public string? NameOrDescription { get; init; }
    public string? Size { get; init; }
    public required decimal Quantity { get; init; }
    public required decimal Amount { get; init; }
    public decimal? Taxable1 { get; init; }
    public decimal? Taxable2 { get; init; }
    public decimal? Taxable3 { get; init; }
    public decimal? Taxable4 { get; init; }
    public decimal? Taxable5 { get; init; }
    public decimal? Taxable6 { get; init; }
    public decimal? Discount1 { get; init; }
    public decimal? Discount2 { get; init; }
    public decimal? Discount3 { get; init; }
    public decimal? Discount4 { get; init; }
    public decimal? Discount5 { get; init; }
    public decimal? Discount6 { get; init; }
    public decimal? Discount7 { get; init; }
    public decimal? Discount8 { get; init; }
    public decimal? Discount9 { get; init; }
    public string? QtyString { get; init; }
    public int? RctPrintGroup { get; init; }
    public int? ItemFlag1 { get; init; }
    public int? ItemFlag2 { get; init; }
    public DateTime? DateTime { get; init; }
    public int? Clerk { get; init; }
    public int? TerminalNumber { get; init; }
    public int? SequenceNumber { get; init; }
    public string? Version { get; init; }

    public bool IsSalesItem => RecordType == 'I';
    public bool IsFunction => RecordType == 'F';
    public bool IsDiscount => RecordType == 'R';
    public bool IsSubtotal => RecordType == 'S';
    public bool IsTender => RecordType == 'T';
    public bool IsTaxRate => RecordType == 'X';
    public bool IsVatRate => RecordType == 'Y';
    public bool IsEndOfSaleMarker => RecordType == 'Z';
}
