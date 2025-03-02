using FluentResults;

namespace PowrIntegration.ZraService.Errors;

public sealed class CircuitBreakerError(string message, Exception exception)
    : ExceptionalError(message, exception);
