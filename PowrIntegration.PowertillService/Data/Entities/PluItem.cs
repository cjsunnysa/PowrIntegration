namespace PowrIntegration.PowertillService.Data.Entities;

public sealed record PluItem
{
    public required long PluNumber { get; init; }
    public string? PluDescription { get; set; }
    public string? SizeDescription { get; set; }
    public decimal SellingPrice1 { get; set; }
    public int SalesGroup { get; set; }
    public string Flags { get; set; } = string.Empty;
    public string? Supplier1StockCode { get; set; }
    public string? Supplier2StockCode { get; set; }
    public DateTime DateTimeCreated { get; set; }
    public DateTime DateTimeEdited { get; set; }
}
