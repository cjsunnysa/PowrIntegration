namespace PowrIntegrationService.Zra;

public abstract record ZraResponse
{
    public required string resultCd { get; init; }           // Result Code
    public required string resultMsg { get; init; }          // Result Message
    public required string resultDt { get; init; }           // Result Date
}
