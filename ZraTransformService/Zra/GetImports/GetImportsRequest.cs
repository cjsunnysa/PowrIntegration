﻿namespace PowrIntegration.Zra.GetImports;

public sealed record GetImportsRequest : ZraRequest
{
    public string lastReqDt { get; init; } = "20160523000000";
}
