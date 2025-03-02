using PowrIntegration.Shared.Observability;

namespace PowrIntegration.PowertillService.Observability;

public class MetricsVariables : IMetrics
{
    public string ApplicationName => "PowrIntegration.PowertillService";
    public string MetricsMeterName => $"{ApplicationName}.Metrics";
}
