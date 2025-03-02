using Microsoft.Extensions.DependencyInjection;
using OpenTelemetry.Exporter;
using OpenTelemetry.Logs;
using OpenTelemetry.Metrics;
using OpenTelemetry.Resources;
using OpenTelemetry.Trace;
using PowrIntegration.Shared.Observability;

namespace PowrIntegration.Shared;


public static class DependencyInjection
{
    public static IServiceCollection ConfigureOpenTelemetry(this IServiceCollection services, IMetrics metrics)
    {
        void configureOtlpExporter(OtlpExporterOptions options) => options.Endpoint = new Uri("http://otel-collector:4317");

        void addPowrIntegration(ResourceBuilder builder) => builder.AddService(metrics.ApplicationName);

        services
            .AddOpenTelemetry()
            .WithMetrics(builder =>
                builder
                    .AddMeter(metrics.MetricsMeterName)
                    .ConfigureResource(addPowrIntegration)
                    .AddRuntimeInstrumentation()
                    .AddProcessInstrumentation()
                    .AddOtlpExporter(configureOtlpExporter))
            .WithTracing(builder =>
                builder
                    .AddSource(metrics.ApplicationName)
                    .ConfigureResource(addPowrIntegration)
                    .AddHttpClientInstrumentation()
                    .AddOtlpExporter(configureOtlpExporter))
            .WithLogging(builder =>
                builder
                    .ConfigureResource(addPowrIntegration)
                    .AddOtlpExporter(configureOtlpExporter));

        return services;
    }
}