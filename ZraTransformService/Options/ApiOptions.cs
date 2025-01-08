namespace PowrIntegration.Options;

public sealed record ApiOptions
{
    public const string KEY = "Services:Api";

    public required string QueueName { get; init; }
    public required string QueueHost { get; init; }
    public required string ApiBaseUrl { get; init; }
    public required string TaxpayerIdentificationNumber { get; init; }
    public required string TaxpayerBranchIdentifier { get; init; }
    public required string DeviceSerialNumber { get; init; }
    public required bool ShouldInitializeDevice { get; init; }
    public required string RegisterDeviceFileName { get; init; }
    public TaxTypeMapping[] TaxTypeMappings { get; set; } = [];
}

public sealed record TaxTypeMapping
{
    public int SalesGroupId { get; set; }
    public string TaxTypeCode { get; set; } = string.Empty;
}