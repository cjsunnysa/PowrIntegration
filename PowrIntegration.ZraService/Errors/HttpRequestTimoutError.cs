using FluentResults;

namespace PowrIntegration.ZraService.Errors;

public sealed class HttpRequestTimoutError(string message, Exception exception)
    : ExceptionalError(message, exception);