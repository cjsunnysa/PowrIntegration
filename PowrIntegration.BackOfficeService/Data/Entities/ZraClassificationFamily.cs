﻿namespace PowrIntegration.BackOfficeService.Data.Entities;

public sealed record ZraClassificationFamily
{
    public required long Code { get; init; }
    public required string Name { get; set; }
    public string? Description { get; set; }
    public required long SegmentCode { get; set; }
    public ZraClassificationSegment? Segment { get; set; }
    public List<ZraClassificationClass> Classes { get; init; } = [];
}
