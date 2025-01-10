using Microsoft.Extensions.Options;
using Polly;
using Polly.Extensions.Http;
using Polly.Timeout;
using PowrIntegration.Options;
using PowrIntegration.Zra;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Net.Http.Json;
using System.Text;
using System.Text.Json;
using System.Threading.Tasks;

namespace PowrIntegration.Extensions;
internal static class DependencyInjection
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

    private static Random _jitterer = new();

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

        services.AddHttpClient<ZraService>((provider, client) =>
            client.BaseAddress = new Uri(provider.GetRequiredService<IOptions<ApiOptions>>().Value.ApiBaseUrl)
        )
        .AddHttpMessageHandler<BufferingHandler>()
        .AddPolicyHandler(ZraCircuitBreakerPolicy)
        .AddPolicyHandler(ZraRetryPolicy)
        .AddPolicyHandler(ZraTimoutPolicy);

        return services;
    }
}
