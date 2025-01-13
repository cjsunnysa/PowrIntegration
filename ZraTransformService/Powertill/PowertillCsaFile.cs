using CsvHelper;
using CsvHelper.Configuration;
using CsvHelper.TypeConversion;
using PowrIntegration.Data.Importers;
using System.Globalization;

namespace PowrIntegration.Powertill;

public sealed class PowertillCsaFile<T>(string filePath, Func<ShouldSkipRecordArgs, bool>? shouldSkipRecord = null, ClassMap<T>? mapping = null)
    : CsaFile<T>(filePath, false, shouldSkipRecord, mapping) where T : class
{ }

public sealed class DelphiDateConverter : DefaultTypeConverter
{
    public override object? ConvertFromString(string? text, IReaderRow row, MemberMapData memberMapData)
    {
        return
            string.IsNullOrWhiteSpace(text) || text == "0"
            ? null
            : int.TryParse(text, NumberStyles.Integer, CultureInfo.InvariantCulture, out int days)
                ? new DateTime(1899, 12, 31).AddDays(days)
                : throw new FormatException($"Invalid date format: {text}");
    }
}

public sealed class Iso8601DateConverter : DefaultTypeConverter
{
    public override object? ConvertFromString(string? text, IReaderRow row, MemberMapData memberMapData)
    {
        return
            string.IsNullOrWhiteSpace(text) || text == "0"
            ? null
            : DateTime.TryParseExact(text, "yyyyMMddTHH:mm:ss", CultureInfo.InvariantCulture, DateTimeStyles.None, out var result)
                ? (object)result
                : throw new FormatException($"Invalid date format: {text}");
    }
}

public sealed class PowertillDateConverter : DefaultTypeConverter
{
    public override object? ConvertFromString(string? text, IReaderRow row, MemberMapData memberMapData)
    {
        return
            string.IsNullOrWhiteSpace(text)
            ? null
            : DateTime.TryParseExact(text, "yyyyMMdd", CultureInfo.InvariantCulture, DateTimeStyles.None, out var result)
                ? (object)result
                : throw new FormatException($"Invalid date format: {text}");
    }
}

public sealed class YesNoConverter : DefaultTypeConverter
{
    public override object? ConvertFromString(string? text, IReaderRow row, MemberMapData memberMapData)
    {
        return text?.Trim() == "Y";
    }
}

