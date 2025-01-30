namespace PowrIntegrationService.MessageQueue;

public enum MessageQueueType
{
    Backoffice = 1,
    Sales = 2,
    Zra = 3
}

public enum QueueMessageType
{
    Sale = 1,
    Purchase = 2,
    ItemInsert = 3,
    ItemUpdate = 4,
    IngredientInsert = 5,
    IngredientUpdate = 6,
    StockTake = 7,
    ZraStandardCodes = 8,
    ZraClassificationCodes = 9,
    ZraImportItems = 10
}
