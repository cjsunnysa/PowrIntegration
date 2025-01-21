using PowrIntegrationService.Zra;
using System.Collections.Immutable;

namespace PowrIntegrationService.Zra.ClassificationCodes;

public sealed record FetchClassificationCodesResponse : ZraResponse
{
    public ImmutableArray<Code> itemClsList { get; init; } = [];

    public class Code
    {
        public string? itemClsCd { get; init; }   // Item Classification Code (UNSPSC)
        public string? itemClsNm { get; init; }   // Item Class Name
        public int? itemClsLvl { get; init; }     // Item Class Level
        public string? taxTyCd { get; init; }     // Taxation Type Code
        public string? mjrTgYn { get; init; }     // Whether it is a Major Target (Y/N)
        public string? useYn { get; init; }       // Used/Unused Flag
    }
}