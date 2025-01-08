using ZraShared.Zra;

namespace ZraSyncService.Zra.ClassificationCodes;

internal sealed class FetchClassificationCodesRequest : ZraRequest
{
    public string lastReqDt { get; init; } = "20231215000000";
}
