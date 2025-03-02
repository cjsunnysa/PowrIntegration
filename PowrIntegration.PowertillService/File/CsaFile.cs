using CsvHelper.Configuration;
using CsvHelper;
using System.Collections.Immutable;
using System.Globalization;

namespace PowrIntegration.PowertillService.File;

public class CsaFile<T>(string filePath)
{
    private readonly string _filePath = filePath;
    private ImmutableArray<T> _cachedRecords;

    public IEnumerable<T> ReadRecords(bool hasHeaderRecord, Func<ShouldSkipRecordArgs, bool>? shouldSkipRecordMethod = null, ClassMap<T>? map = null)
    {
        if (_cachedRecords.IsDefault)
        {
            using var reader = new StreamReader(_filePath);

            var configuration = new CsvConfiguration(CultureInfo.CurrentCulture)
            {
                HasHeaderRecord = hasHeaderRecord,
                HeaderValidated = null,
                ShouldSkipRecord = shouldSkipRecordMethod is not null ? new ShouldSkipRecord(shouldSkipRecordMethod) : null
            };

            using var csv = new CsvReader(reader, configuration);

            if (map is not null)
            {
                csv.Context.RegisterClassMap(map);
            }

            _cachedRecords = csv.GetRecords<T>().ToImmutableArray();
        }

        return _cachedRecords;
    }
}
