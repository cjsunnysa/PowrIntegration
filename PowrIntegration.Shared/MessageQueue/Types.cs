﻿namespace PowrIntegration.Shared.MessageQueue;

public static class MessageQueueHeaderKey
{
    public static string Type = "type";
}

public enum QueueMessageType
{
    Sale = 1,
    Purchase = 2,
    ItemInsert = 3,
    ItemUpdate = 4,
    StockTake = 5,
    ZraStandardCodes = 6,
    ZraClassificationCodes = 7,
    ZraImportItems = 8,
    SavePurchase = 9
}
