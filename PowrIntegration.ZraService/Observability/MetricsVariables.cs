using PowrIntegration.Shared.Observability;

namespace PowrIntegration.ZraService.Observability;

public class MetricsVariables : IMetrics
{
    public string ApplicationName => "PowrIntegration.ZraService";
    public string MetricsMeterName => $"{ApplicationName}.Metrics";
}
