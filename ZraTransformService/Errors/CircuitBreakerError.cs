using FluentResults;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace PowrIntegration.Errors;

public sealed class CircuitBreakerError(string message, Exception exception) 
    : ExceptionalError(message, exception);
