using FluentResults;

namespace PowrIntegration.BackOfficeService.Errors;

public sealed class CircuitBreakerError(string message, Exception exception)
    : ExceptionalError(message, exception);
