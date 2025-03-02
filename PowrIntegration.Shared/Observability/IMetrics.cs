namespace PowrIntegration.Shared.Observability;

public interface IMetrics
{
    string ApplicationName { get; }
    string MetricsMeterName { get; }
}
