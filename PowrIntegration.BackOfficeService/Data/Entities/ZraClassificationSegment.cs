namespace PowrIntegration.BackOfficeService.Data.Entities;

public sealed record ZraClassificationSegment
{
    public required long Code { get; init; }
    public required string Name { get; set; }
    public string? Description { get; set; }
    public List<ZraClassificationFamily> Families { get; init; } = [];
}
