using FluentResults;
using Microsoft.Extensions.Options;
using PowrIntegration.Options;
using System.Collections.Immutable;

namespace PowrIntegration.Data.Importers;

public abstract class FileImporter<T>(IOptions<PowertillOptions> options, string filename, ILogger logger)
{
    protected readonly PowertillOptions Options = options.Value;
    protected readonly string ImportDirectory = options.Value.ImportDirectory;
    protected readonly string FileName = filename;
    private readonly ILogger _logger = logger;
    protected string FilePath => Path.Combine(ImportDirectory, FileName);

    public async Task<Result<ImmutableArray<T>>> Execute(CancellationToken cancellationToken)
    {
        var filePath = FilePath;

        _logger.LogDebug("Checking for {FilePath}.", filePath);

        if (!System.IO.File.Exists(filePath))
        {
            _logger.LogDebug("{FilePath} not found.", filePath);

            return Result.Ok(ImmutableArray<T>.Empty);
        }

        _logger.LogInformation("Importing {FilePath}.", filePath);

        var importResult = await ExecuteImport(cancellationToken);

        if (importResult.IsFailed)
        {
            return importResult.ToResult();
        }

        MoveFileToHistory(filePath);

        return Result.Ok(importResult.Value);
    }

    private void MoveFileToHistory(string filePath)
    {
        var historyFileName = $"{Path.GetFileNameWithoutExtension(FileName)}_{DateTime.Now:yyyyMMddhhmmssfff}{Path.GetExtension(FileName)}";

        var historyDirectory = Path.Combine(ImportDirectory, "History");

        if (!Directory.Exists(historyDirectory))
        {
            Directory.CreateDirectory(historyDirectory);
        }

        var historyFilePath = Path.Combine(historyDirectory, historyFileName);

        System.IO.File.Move(filePath, historyFilePath);
    }

    protected abstract Task<Result<ImmutableArray<T>>> ExecuteImport(CancellationToken cancellationToken);
}
