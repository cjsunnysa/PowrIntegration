﻿namespace PowrIntegrationService.Zra.InitializeDevice;

public sealed record InitializeRequest : ZraRequest
{
    public required string dvcSrlNo { get; init; }
}
