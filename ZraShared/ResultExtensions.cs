using FluentResults;
using Microsoft.Extensions.Logging;

namespace ZraShared;

public static class ResultExtensions
{
    public static Result LogErrors(this Result result, ILogger logger)
    {
        foreach (var error in result.Errors)
        {
            LogError(error, logger);
        }

        return result;
    }

    public static Result<T> LogErrors<T>(this Result<T> result, ILogger logger)
    {
        foreach (var error in result.Errors)
        {
            LogError(error, logger);
        }

        return result;
    }

    private static void LogError(IError error, ILogger logger)
    {
        if (error is ExceptionalError exError)
        {
            logger.LogError(exError.Exception, message: exError.Message);
            return;
        }

        logger.LogError("Error: {ErrorMessage}", error.Message);
    }
}
