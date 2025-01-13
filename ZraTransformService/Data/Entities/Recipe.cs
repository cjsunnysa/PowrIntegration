namespace PowrIntegration.Data.Entities;

public sealed record Recipe
{
    public required long PluNumber { get; set; } // Max 15 digits
    public required decimal Portions { get; set; }

    public PluItem? Plu { get; set; }
    public List<Ingredient> Ingredients { get; set; } = [];
}
