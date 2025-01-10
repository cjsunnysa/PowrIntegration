using FluentResults;

namespace PowrIntegration.Errors;

public sealed class HttpRequestTimoutError(string message, Exception exception)
    : ExceptionalError(message, exception);