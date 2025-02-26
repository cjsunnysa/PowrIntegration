﻿using PowrIntegration.Shared.MessageQueue;

namespace PowrIntegrationService.Data.Entities;

public sealed record OutboxItem
{
    public int? Id { get; init; }
    public required QueueMessageType MessageType { get; init; }
    public required byte[] MessageBody { get; init; }
    public byte FailureCount { get; set; }
}