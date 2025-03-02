using FluentResults;

namespace PowrIntegration.PowertillService.Errors;

public sealed class CircuitBreakerError(string message, Exception exception)
    : ExceptionalError(message, exception);
