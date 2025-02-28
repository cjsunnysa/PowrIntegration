using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Options;
using OpenTelemetry.Exporter;
using OpenTelemetry.Logs;
using OpenTelemetry.Metrics;
using OpenTelemetry.Resources;
using OpenTelemetry.Trace;
using Polly;
using Polly.Extensions.Http;
using Polly.Timeout;
using PowrIntegrationService.Data;
using PowrIntegrationService.Options;
using PowrIntegrationService.Zra;
using System.Text.Json;

namespace PowrIntegrationService.Extensions;
internal static class Startup
{
    private sealed class ZraResponse
    {
        public required string resultCd { get; init; }
        public required string resultMsg { get; init; }
    }

    // handler for buffering the Http response content stream so it can be read multiple times
    private sealed class BufferingHandler : DelegatingHandler
    {
        protected override async Task<HttpResponseMessage> SendAsync(HttpRequestMessage request, CancellationToken cancellationToken)
        {
            HttpResponseMessage response = await base.SendAsync(request, cancellationToken);

            if (response.Content is null)
            {
                return response;
            }

            string content = await response.Content.ReadAsStringAsync(cancellationToken);

            response.Content = new StringContent(content);

            return response;
        }
    }

    private sealed class LoggingHandler : DelegatingHandler
    {
        private readonly ILogger<LoggingHandler> _logger;

        public LoggingHandler(ILogger<LoggingHandler> logger)
        {
            _logger = logger;
        }

        protected override async Task<HttpResponseMessage> SendAsync(HttpRequestMessage request, CancellationToken cancellationToken)
        {
            _logger.LogInformation("Request: {HttpMethod} {HttpRequestUri}", request.Method, request.RequestUri);

            if (request.Content != null)
            {
                var requestBody = await request.Content.ReadAsStringAsync();

                _logger.LogInformation("Request Body: {HttpBody}", requestBody);
            }

            var response = await base.SendAsync(request, cancellationToken);

            _logger.LogInformation("Response: {HttpResponseStatusCode}", response.StatusCode);

            if (response.Content != null)
            {
                var responseBody = await response.Content.ReadAsStringAsync();

                _logger.LogInformation("Response Body: {HttpResponseBody}", responseBody);
            }

            return response;
        }
    }

    private static readonly Random _jitterer = new();

    private static IAsyncPolicy<HttpResponseMessage> ZraRetryPolicy =>
        HttpPolicyExtensions
            .HandleTransientHttpError()
            .Or<TimeoutRejectedException>()
            .OrResult(IsZraServerCommunicationError)
            .WaitAndRetryAsync(3, retryAttempt => TimeSpan.FromSeconds(Math.Pow(2, retryAttempt) + _jitterer.NextDouble()));

    private static IAsyncPolicy<HttpResponseMessage> ZraTimoutPolicy =>
        Policy.TimeoutAsync<HttpResponseMessage>(TimeSpan.FromSeconds(5));

    private static IAsyncPolicy<HttpResponseMessage> ZraCircuitBreakerPolicy =>
        HttpPolicyExtensions
            .HandleTransientHttpError()
            .Or<TimeoutRejectedException>()
            .OrResult(IsZraServerCommunicationError)
            .CircuitBreakerAsync(2, TimeSpan.FromMinutes(1));

    private static bool IsZraServerCommunicationError(HttpResponseMessage response)
    {
        // Check if the response's resultCd is 802, 838, or 894
        // 802 Data Not Transferred
        // 838 Connection Error
        // 894 Server Communication Error
        string stringResponse = response.Content.ReadAsStringAsync(CancellationToken.None).GetAwaiter().GetResult();

        var zraResponse = JsonSerializer.Deserialize<ZraResponse>(stringResponse);

        return zraResponse?.resultCd == "802" || zraResponse?.resultCd == "838" || zraResponse?.resultCd == "894";
    }


    public static IServiceCollection ConfigureHttpClients(this IServiceCollection services)
    {
        services.AddSingleton<BufferingHandler>();
        services.AddSingleton<LoggingHandler>();

        services.AddHttpClient<ZraService>((provider, client) =>
            client.BaseAddress = new Uri(provider.GetRequiredService<IOptions<ZraApiOptions>>().Value.BaseUrl)
        )
        .AddHttpMessageHandler<BufferingHandler>()
        .AddHttpMessageHandler<LoggingHandler>()
        .AddPolicyHandler(ZraCircuitBreakerPolicy)
        .AddPolicyHandler(ZraRetryPolicy)
        .AddPolicyHandler(ZraTimoutPolicy);

        return services;
    }

    public static IServiceCollection ConfigureOpenTelemetry(this IServiceCollection services)
    {
        static void configureOtlpExporter(OtlpExporterOptions options) => options.Endpoint = new Uri("http://otel-collector:4317");

        static void addPowrIntegration(ResourceBuilder builder) => builder.AddService(Metrics.ApplicationName);

        services
            .AddOpenTelemetry()
            .WithMetrics(builder =>
                builder
                    .AddMeter(Metrics.MetricsMeterName)
                    .ConfigureResource(addPowrIntegration)
                    .AddRuntimeInstrumentation()
                    .AddProcessInstrumentation()
                    .AddOtlpExporter(configureOtlpExporter))
            .WithTracing(builder =>
                builder
                    .AddSource(Metrics.ApplicationName)
                    .ConfigureResource(addPowrIntegration)
                    .AddHttpClientInstrumentation()
                    .AddOtlpExporter(configureOtlpExporter))
            .WithLogging(builder =>
                builder
                    .ConfigureResource(addPowrIntegration)
                    .AddOtlpExporter(configureOtlpExporter));

        return services;
    }

    public static IServiceCollection ConfigureEntityFramework(this IServiceCollection services, string connectionString)
    {
        return services.AddDbContextFactory<PowrIntegrationDbContext>(options => options.UseSqlite(connectionString));
    }

    public static IServiceCollection ConfigurePowrIntegrationOptions(this IServiceCollection services, ConfigurationManager config)
    {
        services.Configure<BackOfficeServiceOptions>(config);
        services.Configure<RabbitMqOptions>(config.GetSection(RabbitMqOptions.KEY));
        services.Configure<ZraApiOptions>(config.GetSection(ZraApiOptions.KEY));
        services.Configure<PowertillOptions>(config.GetSection(PowertillOptions.KEY));

        return services;
    }

    public static IServiceProvider ApplyDatabaseMigrations(this IServiceProvider serviceProvider)
    {
        using var scope = serviceProvider.CreateScope();

        var services = scope.ServiceProvider;

        try
        {
            var context = services.GetRequiredService<PowrIntegrationDbContext>();

            context.Database.Migrate(); // Create or update the database schema
        }
        catch (Exception ex)
        {
            var loggerFactory = serviceProvider.GetRequiredService<ILoggerFactory>();

            var logger = loggerFactory.CreateLogger("Startup");

            logger.LogError(ex, "An error ocurred attempting to apply database migrations.");
        }

        return serviceProvider;
    }
}
