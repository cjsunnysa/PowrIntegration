using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace PowrIntegration.Data.Entities;

public sealed record Ingredient
{
    public long PluNumber { get; init; }
    public required long IngredientNumber { get; set; } // 0 for recipe header
    public decimal? IngredientQuantity { get; set; } // #.###, nullable based on examples
    public decimal? UnitStockRatio { get; set; } // #.###, nullable based on examples
    public decimal? RecipeNettCost { get; set; } // $.$$, nullable for recipe header
    public decimal? RecipeGrossCost { get; set; } // $.$$, nullable for recipe header
    public string? NewPluNumber { get; set; } // ###########, nullable for recipe header

    public Recipe? Recipe { get; init; }
    public PluItem? IngredientPlu { get; set; }
}
