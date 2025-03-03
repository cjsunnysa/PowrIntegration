using System.Collections.Immutable;

namespace PowrIntegration.BackOfficeService.Options;

internal sealed class ZraApiOptions
{
    public const string KEY = "ZraApi";

    public ImmutableArray<TaxMapping> TaxMappings { get; init; } = [];
}

internal sealed class TaxMapping
{
    public required int SalesGroupId { get; init; }
    public required int TaxGroupId { get; init; }
    public required string TaxTypeCode { get; init; }
}
