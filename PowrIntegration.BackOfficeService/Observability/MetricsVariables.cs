using PowrIntegration.Shared.Observability;

namespace PowrIntegration.BackOfficeService.Observability;

public class MetricsVariables : IMetrics
{
    public string ApplicationName => "PowrIntegration.BackOfficeService";
    public string MetricsMeterName => $"{ApplicationName}.Metrics";
}

