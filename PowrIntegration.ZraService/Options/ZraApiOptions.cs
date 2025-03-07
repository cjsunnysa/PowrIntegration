﻿namespace PowrIntegration.ZraService.Options;

public sealed record ZraApiOptions
{
    public const string KEY = "ZraApi";

    public required string BaseUrl { get; init; }
    public required string TaxpayerIdentificationNumber { get; init; }
    public required string TaxpayerBranchIdentifier { get; init; }
    public required string DeviceSerialNumber { get; init; }
    public required bool ShouldInitializeDevice { get; init; }
    public required string RegisterDeviceFileName { get; init; }
    public required TaxMapping[] TaxMappings { get; init; }
}

public sealed record TaxMapping
{
    public required int SalesGroupId { get; init; }
    public required int TaxGroupId { get; init; }
    public required string TaxTypeCode { get; init; }
}