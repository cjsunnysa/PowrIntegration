namespace PowrIntegrationService.MessageQueue;

public enum MessageQueueType
{
    Backoffice = 1,
    Zra = 2
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
    ZraImportItems = 8
}
