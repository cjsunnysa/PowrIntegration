using System.Collections.Immutable;

namespace PowrIntegration.Dtos;

public sealed record StandardCodeClassDto
{
    public required string Code { get; init; }
    public required string Name { get; init; }

    public ImmutableArray<StandardCodeDto> StandardCodes { get; init; } = [];
}

public sealed record StandardCodeDto
{
    public required string Code { get; init; }
    public required string Name { get; init; }
    public string? UserDefinedName { get; init; }
}
