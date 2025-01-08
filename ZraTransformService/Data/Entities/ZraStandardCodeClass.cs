namespace PowrIntegration.Data.Entities;

public sealed record ZraStandardCodeClass
{
    public required string Code { get; init; }        // Code Class
    public required string Name { get; set; }      // Code Class Name
    public List<ZraStandardCode> Codes { get; init; } = [];
}
