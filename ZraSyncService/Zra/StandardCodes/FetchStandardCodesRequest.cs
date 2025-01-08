using ZraShared.Zra;

namespace ZraSyncService.Zra.StandardCodes;

internal sealed class FetchStandardCodesRequest : ZraRequest
{
    public string lastReqDt { get; init; } = "20231215000000";
}
