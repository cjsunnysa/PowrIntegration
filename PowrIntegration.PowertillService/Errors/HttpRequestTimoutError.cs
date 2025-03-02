using FluentResults;

namespace PowrIntegration.PowertillService.Errors;

public sealed class HttpRequestTimoutError(string message, Exception exception)
    : ExceptionalError(message, exception);