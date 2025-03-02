namespace PowrIntegration.PowertillService.Data.Entities;

public sealed record ZraStandardCode
{
    public required string Code { get; init; }           // Standard Code
    public required string Name { get; set; }         // Standard Code Name
    public string? UserDefinedName { get; set; }   // User Define Name 1 (Optional)
    public required string ClassCode { get; set; }
    public ZraStandardCodeClass? Class { get; set; }
}
