using CsvHelper.Configuration;
using EFCore.BulkExtensions;
using FluentResults;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Options;
using PowrIntegrationService.Data.Entities;
using PowrIntegrationService.Extensions;
using PowrIntegrationService.Options;
using PowrIntegrationService.Powertill;
using System.Collections.Immutable;
using System.Diagnostics.Metrics;

namespace PowrIntegrationService.Data.Importers;

public sealed class IngredientsImport(IOptions<IntegrationServiceOptions> options, IDbContextFactory<PowrIntegrationDbContext> dbContextFactory, ILogger<IngredientsImport> logger)
    : FileImporter<Recipe>(options, "IngCreat.csa", logger)
{
    private readonly IDbContextFactory<PowrIntegrationDbContext> _dbContextFactory = dbContextFactory;

    protected async override Task<Result<ImmutableArray<Recipe>>> ExecuteImport(CancellationToken cancellationToken)
    {
        try
        {
            IncrementTimesImportedMetric();

            var ingredientMap = new IngredientMap();

            var csaFile = new PowertillCsaFile<IngredientDto>(FilePath, null, ingredientMap);

            var dtos = csaFile.ReadRecords().ToImmutableArray();

            var ingredientGroups = dtos.GroupBy(x => x.PluNumber);

            var recipes = ingredientGroups.MapToRecipes();

            var ingredients = ingredientGroups.MapToIngredients();

            using var dbContext = await _dbContextFactory.CreateDbContextAsync(cancellationToken);

            await dbContext.BulkInsertOrUpdateAsync(recipes, cancellationToken: cancellationToken);

            await dbContext.BulkInsertOrUpdateAsync(ingredients, cancellationToken: cancellationToken);

            await dbContext.SaveChangesAsync(cancellationToken);

            return Result.Ok(recipes);
        }
        catch (Exception ex)
        {
            return Result.Fail(new ExceptionalError($"An error occured importing PLU items from {FilePath}.", ex));
        }
    }

    private static void IncrementTimesImportedMetric()
    {
        var meter = new Meter(PowrIntegrationValues.MetricsMeterName);

        var counter = meter.CreateCounter<long>("ingredient_file_import_counter", "times", "Counts number of times ingredients imported from IngCreat file.");

        counter.Add(1);
    }
}

public sealed class IngredientMap : ClassMap<IngredientDto>
{
    public IngredientMap()
    {
        Map(m => m.PluNumber).Index(0); // Maps to column 1 (0-based index)
        Map(m => m.IngredientNumber).Index(1); // Maps to column 2
        Map(m => m.IngredientQuantity).Index(2).TypeConverterOption.NullValues("", "null"); // Maps to column 3
        Map(m => m.UnitStockRatio).Index(3).TypeConverterOption.NullValues("", "null"); // Maps to column 4
        Map(m => m.RecipeNettCost).Index(4).TypeConverterOption.NullValues("", "null"); // Maps to column 5
        Map(m => m.RecipeGrossCost).Index(5).TypeConverterOption.NullValues("", "null"); // Maps to column 6
        Map(m => m.NewPluNumber).Index(13).TypeConverterOption.NullValues("", "null"); // Maps to column 22
    }
}
