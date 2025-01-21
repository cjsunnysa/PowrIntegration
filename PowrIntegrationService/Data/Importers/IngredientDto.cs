namespace PowrIntegrationService.Data.Importers;

public sealed record IngredientDto
{
    public required long PluNumber { get; set; }
    public required long IngredientNumber { get; set; } // 0 for recipe header
    public decimal? IngredientQuantity { get; set; } // #.###, nullable based on examples
    public decimal? UnitStockRatio { get; set; } // #.###, nullable based on examples
    public decimal? RecipeNettCost { get; set; } // $.$$, nullable for recipe header
    public decimal? RecipeGrossCost { get; set; } // $.$$, nullable for recipe header
    public string? NewPluNumber { get; set; } // ###########, nullable for recipe header

    public bool IsHeader => IngredientNumber == 0;
}
