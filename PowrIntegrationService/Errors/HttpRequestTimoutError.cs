﻿using FluentResults;

namespace PowrIntegrationService.Errors;

public sealed class HttpRequestTimoutError(string message, Exception exception)
    : ExceptionalError(message, exception);