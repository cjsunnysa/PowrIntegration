using ZraShared.Zra;

namespace ZraSyncService.Zra.InitializeDevice;

internal sealed class InitializeRequest : ZraRequest
{
    public required string dvcSrlNo { get; init; }
}
