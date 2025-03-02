using FluentResults;
using System.Text;
using System.Text.Json;

namespace PowrIntegration.Shared.File;

public class TextFile(string filename)
{
    private readonly string _filename = filename;

    public async Task<Result> Write<T>(T objectToWrite, CancellationToken cancellationToken)
    {
        try
        {
            using var stream = new MemoryStream();

            await JsonSerializer.SerializeAsync(stream , objectToWrite, cancellationToken: cancellationToken);

            var content = Encoding.UTF8.GetString(stream.ToArray());

            return await Write(content, cancellationToken);
        }
        catch (Exception ex)
        {
            return Result.Fail(new ExceptionalError($"An error occurred writing an object to file {_filename}.", ex));
        }
    }

    public async Task<Result> Write(string content)
    {
        try
        {
            await System.IO.File.WriteAllTextAsync(_filename, content);

            return Result.Ok();
        }
        catch (Exception ex)
        {
            return Result.Fail(new ExceptionalError($"An error occurred writing file {_filename}.", ex));
        }
    }
}
