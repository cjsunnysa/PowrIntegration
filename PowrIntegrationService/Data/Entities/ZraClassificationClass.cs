namespace PowrIntegrationService.Data.Entities;

public sealed record ZraClassificationClass
{
    public required long Code { get; init; }
    public required string Name { get; set; }
    public string? Description { get; set; }
    public required long FamilyCode { get; set; }
    public ZraClassificationFamily? Family { get; set; }
    public List<ZraClassificationCode> Codes { get; init; } = [];
}
