namespace PowrIntegrationService.Dtos;
public sealed record ClassificationCodeDto
{
    public string? Code { get; init; }
    public string? Name { get; init; }
    public int? Level { get; init; }
    public string? TaxTypeCode { get; init; }
    public bool? IsMajorTarget { get; init; }
    public bool? ShouldUse { get; init; }
}
