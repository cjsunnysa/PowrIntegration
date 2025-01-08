using ZraShared.Zra;

namespace ZraSyncService.Zra.InitializeDevice;

public sealed class InitializationResponse : ZraResponse
{
    public string? tpin { get; init; }               // Taxpayer Identification Number
    public string? taxprNm { get; init; }            // Taxpayer Name
    public string? bsnsActv { get; init; }           // Business Activity
    public string? bhfId { get; init; }              // Branch Office Id
    public string? bhfNm { get; init; }              // Branch Office Name
    public string? bhfOpenDt { get; init; }          // Branch Date Created
    public string? prvncNm { get; init; }            // Province Name
    public string? dstrtNm { get; init; }            // District Name
    public string? sctrNm { get; init; }             // Sector Name
    public string? locDesc { get; init; }            // Location Description
    public string? hqYn { get; init; }               // Head Quarter
    public string? mgrNm { get; init; }              // Manager Name
    public string? mgrTelNo { get; init; }           // Manager Contact Number
    public string? mgrEmail { get; init; }           // Manager Email
    public string? sdicId { get; init; }             // SDC id
    public string? mrcNo { get; init; }              // MRC No
    public long? lastSaleInvcNo { get; init; }       // Last Sale Invoice Number
    public long? lastPchsInvcNo { get; init; }       // Last Purchase Invoice Number
    public long? lastSaleRcptNo { get; init; }       // Last Sale Receipt Number
    public long? lastInvcNo { get; init; }           // Last CIS Invoice Number
    public long? lastTrainInvcNo { get; init; }      // Last Training Invoice Number
    public long? lastProfrmInvcNo { get; init; }     // Last Proforma Invoice Number
    public long? lastCopyInvcNo { get; init; }       // Last Copy Invoice Number
}
