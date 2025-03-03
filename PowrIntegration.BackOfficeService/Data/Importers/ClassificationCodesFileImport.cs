using CsvHelper.Configuration;
using System.Collections.Immutable;
using FluentResults;
using Microsoft.Extensions.Options;
using Microsoft.EntityFrameworkCore;
using EFCore.BulkExtensions;
using System.Diagnostics.Metrics;
using PowrIntegration.PowertillService.Data.Entities;
using PowrIntegration.Shared.Observability;
using PowrIntegration.BackOfficeService.Data.Entities;
using PowrIntegration.BackOfficeService.Data;
using PowrIntegration.BackOfficeService.File;
using PowrIntegration.BackOfficeService.Options;

namespace PowrIntegration.BackOfficeService.Data.Importers;

public sealed class ClassificationCodesFileImport(
    IOptions<BackOfficeServiceOptions> options,
    IDbContextFactory<PowrIntegrationDbContext> dbContextFactory,
    IMetrics metrics,
    ILogger<ClassificationCodesFileImport> logger)
    : FileImporter<ZraClassificationCode>(options, "UNSPSC-Classification-Codes.csv", logger)
{
    private readonly IDbContextFactory<PowrIntegrationDbContext> _dbContextFactory = dbContextFactory;
    private readonly IMetrics _metrics = metrics;

    public sealed record CsaFileRow
    {
        public string Version { get; init; } = string.Empty;
        public long Key { get; init; }
        public long Segment { get; init; }
        public string SegmentTitle { get; init; } = string.Empty;
        public string SegmentDefinition { get; init; } = string.Empty;
        public long Family { get; init; }
        public string FamilyTitle { get; init; } = string.Empty;
        public string FamilyDefinition { get; init; } = string.Empty;
        public long Class { get; init; }
        public string ClassTitle { get; init; } = string.Empty;
        public string ClassDefinition { get; init; } = string.Empty;
        public long Commodity { get; init; }
        public string CommodityTitle { get; init; } = string.Empty;
        public string CommodityDefinition { get; init; } = string.Empty;
        public string Synonym { get; init; } = string.Empty;
        public string Acronym { get; init; } = string.Empty;
    }

    public sealed class CsaFileRowMap : ClassMap<CsaFileRow>
    {
        public CsaFileRowMap()
        {
            Map(x => x.Version).Name("Version");
            Map(x => x.Key).Name("Key");
            Map(x => x.Segment).Name("Segment");
            Map(x => x.SegmentTitle).Name("Segment Title");
            Map(x => x.SegmentDefinition).Name("Segment Definition");
            Map(x => x.Family).Name("Family");
            Map(x => x.FamilyTitle).Name("Family Title");
            Map(x => x.FamilyDefinition).Name("Family Definition");
            Map(x => x.Class).Name("Class");
            Map(x => x.ClassTitle).Name("Class Title");
            Map(x => x.ClassDefinition).Name("Class Definition");
            Map(x => x.Commodity).Name("Commodity");
            Map(x => x.CommodityTitle).Name("Commodity Title");
            Map(x => x.CommodityDefinition).Name("Commodity Definition");
        }
    }

    protected override async Task<Result<ImmutableArray<ZraClassificationCode>>> Import(CancellationToken cancellationToken)
    {
        try
        {
            IncrementTimesImportedMetric();

            var filePath = FilePath;

            var map = new CsaFileRowMap();

            var csaFile = new CsaFile<CsaFileRow>(filePath);

            var csaRows = csaFile.ReadRecords(
                hasHeaderRecord: true,
                shouldSkipRecordMethod:
                    args =>
                        args.Row[2] == string.Empty ||
                        args.Row[5] == string.Empty ||
                        args.Row[8] == string.Empty ||
                        args.Row[11] == string.Empty,
                map: map);

            var segments = csaRows.DistinctBy(x => x.Segment).Select(x => new ZraClassificationSegment { Code = x.Segment, Name = x.SegmentTitle, Description = x.SegmentDefinition }).ToImmutableArray();
            var families = csaRows.DistinctBy(x => x.Family).Select(x => new ZraClassificationFamily { Code = x.Family, Name = x.FamilyTitle, Description = x.FamilyDefinition, SegmentCode = x.Segment }).ToImmutableArray();
            var classes = csaRows.DistinctBy(x => x.Class).Select(x => new ZraClassificationClass { Code = x.Class, Name = x.ClassTitle, Description = x.ClassDefinition, FamilyCode = x.Family }).ToImmutableArray();
            var codes = csaRows.DistinctBy(x => x.Commodity).Select(x => new ZraClassificationCode { Code = x.Commodity, Name = x.CommodityTitle, Description = x.CommodityDefinition, ClassCode = x.Class, ShouldUse = true }).ToImmutableArray();

            using var dbContext = await _dbContextFactory.CreateDbContextAsync(cancellationToken);

            await dbContext.BulkInsertOrUpdateAsync(segments, cancellationToken: cancellationToken);
            await dbContext.BulkInsertOrUpdateAsync(families, cancellationToken: cancellationToken);
            await dbContext.BulkInsertOrUpdateAsync(classes, cancellationToken: cancellationToken);
            await dbContext.BulkInsertOrUpdateAsync(codes, cancellationToken: cancellationToken);

            await dbContext.SaveChangesAsync(cancellationToken);

            return Result.Ok(codes);
        }
        catch (Exception ex)
        {
            return Result.Fail(new ExceptionalError($"An error occured importing classifcation codes from {FilePath}", ex));
        }
    }

    private void IncrementTimesImportedMetric()
    {
        var meter = new Meter(_metrics.MetricsMeterName);

        var counter = meter.CreateCounter<long>("classification_codes_file_import_counter", "times", "Counts number of times UN classification codes imported from file.");

        counter.Add(1);
    }
}
