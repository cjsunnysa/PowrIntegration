using FluentResults;

namespace PowrIntegrationService.Errors;

public sealed class CircuitBreakerError(string message, Exception exception)
    : ExceptionalError(message, exception);
