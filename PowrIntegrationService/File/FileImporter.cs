using FluentResults;
using Microsoft.Extensions.Options;
using PowrIntegration.Shared.Extensions;
using PowrIntegrationService.Options;
using System.Collections.Immutable;

namespace PowrIntegrationService.File;

public abstract class FileImporter<T>(IOptions<BackOfficeServiceOptions> options, string filename, ILogger logger)
{
    protected readonly BackOfficeServiceOptions Options = options.Value;
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

        var importResult = await Import(cancellationToken);

        importResult.LogErrors(_logger);

        if (importResult.IsFailed)
        {
            return importResult.ToResult();
        }

        MoveFileToHistory(filePath);

        _logger.LogInformation("Records successfully imported from {ImportFile}.", FilePath);

        return Result.Ok(importResult.Value);
    }

    private void MoveFileToHistory(string filePath)
    {
        try
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
        catch (Exception ex)
        {
            _logger.LogError(ex, "An error occured moving file {ImportFile} to history.", FilePath);
        }
    }

    protected abstract Task<Result<ImmutableArray<T>>> Import(CancellationToken cancellationToken);
}
