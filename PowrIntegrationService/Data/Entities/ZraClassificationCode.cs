namespace PowrIntegrationService.Data.Entities;

public sealed record ZraClassificationCode
{
    public required long Code { get; init; }   // Item Classification Code (UNSPSC)
    public required string Name { get; set; }   // Item Class Name
    public string? Description { get; set; }
    public string? TaxTypeCode { get; set; }     // Taxation Type Code
    public bool IsMajorTarget { get; set; }     // Whether it is a Major Target (Y/N)
    public bool ShouldUse { get; set; }       // Used/Unused Flag
    public long? ClassCode { get; set; }
    public ZraClassificationClass? Class { get; set; }
}
