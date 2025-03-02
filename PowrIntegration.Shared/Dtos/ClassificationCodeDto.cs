namespace PowrIntegration.Shared.Dtos;
public sealed record ClassificationCodeDto
{
    public enum ClassificationLevel
    {
        Segment = 1,
        Family = 2,
        Class = 3,
        Commodity = 4
    }

    public string? Code { get; init; }
    public string? Name { get; init; }
    public int? Level { get; init; }
    public string? TaxTypeCode { get; init; }
    public bool? IsMajorTarget { get; init; }
    public bool? ShouldUse { get; init; }
}
