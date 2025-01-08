namespace PowrIntegration.Zra;

public abstract record ZraRequest
{
    public required string tpin { get; init; }
    public required string bhfId { get; init; }
}
