using Microsoft.Extensions.Options;
using Polly;
using Polly.Extensions.Http;
using Polly.Timeout;
using PowrIntegration.Shared.Options;
using PowrIntegration.ZraService.Options;
using PowrIntegration.ZraService.Zra;
using System.Text.Json;

namespace PowrIntegration.ZraService;

internal static class DependencyInjection
{
    private sealed class ZraResponseForRetryPolicy
    {
        public string? resultCd { get; init; }
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

    private sealed class LoggingHandler(ILogger<LoggingHandler> logger) : DelegatingHandler
    {
        private readonly ILogger<LoggingHandler> _logger = logger;

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

        var zraResponse = JsonSerializer.Deserialize<ZraResponseForRetryPolicy>(stringResponse);

        return zraResponse?.resultCd == "802" || zraResponse?.resultCd == "838" || zraResponse?.resultCd == "894";
    }


    public static IServiceCollection ConfigureHttpClients(this IServiceCollection services)
    {
        services.AddSingleton<BufferingHandler>();
        services.AddSingleton<LoggingHandler>();

        services.AddHttpClient<ZraRestService>((provider, client) =>
            client.BaseAddress = new Uri(provider.GetRequiredService<IOptions<ZraApiOptions>>().Value.BaseUrl)
        )
        .AddHttpMessageHandler<BufferingHandler>()
        .AddHttpMessageHandler<LoggingHandler>()
        .AddPolicyHandler(ZraCircuitBreakerPolicy)
        .AddPolicyHandler(ZraRetryPolicy)
        .AddPolicyHandler(ZraTimoutPolicy);

        return services;
    }

    public static IServiceCollection ConfigureServiceOptions(this IServiceCollection services, ConfigurationManager config)
    {
        services.Configure<RabbitMqOptions>(config.GetSection(RabbitMqOptions.KEY));
        services.Configure<ZraApiOptions>(config.GetSection(ZraApiOptions.KEY));
        services.Configure<ServiceOptions>(config.GetSection(ServiceOptions.KEY));

        return services;
    }
}
