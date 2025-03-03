using FluentResults;

namespace PowrIntegration.BackOfficeService.Errors;

public sealed class HttpRequestTimoutError(string message, Exception exception)
    : ExceptionalError(message, exception);