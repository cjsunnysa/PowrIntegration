using PowrIntegrationService.Zra;
using System.Collections.Immutable;

namespace PowrIntegrationService.Zra.StandardCodes;

public sealed record FetchStandardCodesResponse : ZraResponse
{
    public ImmutableArray<CodeClass> clsList { get; init; } = [];

    public class CodeClass
    {
        public required string cdCls { get; init; }        // Code Class
        public required string cdClsNm { get; init; }      // Code Class Name
        public ImmutableArray<CodeDetail> dtlList { get; init; } = [];
    }

    public class CodeDetail
    {
        public required string cd { get; init; }           // Standard Code
        public required string cdNm { get; init; }         // Standard Code Name
        public string? userDfnNm1 { get; init; }   // User Define Name 1 (Optional)
    }
}
