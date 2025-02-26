using AutoFixture;
using FluentResults;
using Microsoft.Extensions.Options;
using Polly.CircuitBreaker;
using Polly.Timeout;
using PowrIntegrationService.Dtos;
using PowrIntegrationService.Errors;
using PowrIntegrationService.Extensions;
using PowrIntegrationService.Options;
using PowrIntegrationService.Zra.ClassificationCodes;
using PowrIntegrationService.Zra.GetImports;
using PowrIntegrationService.Zra.GetPurchases;
using PowrIntegrationService.Zra.InitializeDevice;
using PowrIntegrationService.Zra.SaveItem;
using PowrIntegrationService.Zra.SavePurchase;
using PowrIntegrationService.Zra.StandardCodes;
using PowrIntegrationService.Zra.UpdateItem;
using System.Collections.Immutable;
using System.Net.Http.Json;

namespace PowrIntegrationService.Zra;

public class ZraService(HttpClient httpClient, IOptions<ZraApiOptions> apiOptions, ILogger<ZraService> logger)
{
    private readonly HttpClient _httpClient = httpClient;
    private readonly ZraApiOptions _apiOptions = apiOptions.Value;
    private readonly ILogger<ZraService> _logger = logger;

    public async Task<Result<InitializationResponse>> InitializeDevice(CancellationToken cancellationToken)
    {
        try
        {
            _logger.LogInformation("Sending Initialize Device request to the ZRA API.");

            var request = new InitializeRequest
            {
                tpin = apiOptions.Value.TaxpayerIdentificationNumber,
                bhfId = apiOptions.Value.TaxpayerBranchIdentifier,
                dvcSrlNo = apiOptions.Value.DeviceSerialNumber
            };

            HttpResponseMessage httpResponse = await _httpClient.PostAsJsonAsync("initializer/selectInitInfo", request, cancellationToken: cancellationToken);

            if (!httpResponse.IsSuccessStatusCode)
            {
                return Result.Fail(new Error($"Error response from ZRA API Initialize Device request. Status: {httpResponse.StatusCode} Reason: {httpResponse.ReasonPhrase}."));
            }

            var response = await httpResponse.Content.ReadFromJsonAsync<InitializationResponse>(cancellationToken: cancellationToken);

            if (response is null)
            {
                string body = await httpResponse.Content.ReadAsStringAsync(cancellationToken);

                return Result.Fail(new Error($"Unexepected response content from ZRA API Initialize Device request. ResponseBody: {body}"));
            }

            return Result.Ok(response);
        }
        catch (TimeoutRejectedException tEx)
        {
            return Result.Fail(new HttpRequestTimoutError("ZRA API Initialize Device request timed-out.", tEx));
        }
        catch (BrokenCircuitException cEx)
        {
            return Result.Fail(new CircuitBreakerError("ZRA API Initialize Device request failed due to the circuit breaker.", cEx));
        }
        catch (Exception ex)
        {
            return Result.Fail(new ExceptionalError(ex));
        }
    }

    public async Task<Result<ImmutableArray<StandardCodeClassDto>>> FetchStandardCodes(CancellationToken cancellationToken)
    {
        try
        {
            _logger.LogInformation("Sending Fetch Standard Codes request to the ZRA API.");

            var request = new FetchStandardCodesRequest
            {
                tpin = _apiOptions.TaxpayerIdentificationNumber,
                bhfId = _apiOptions.TaxpayerBranchIdentifier,
            };

            HttpResponseMessage httpResponse = await _httpClient.PostAsJsonAsync("code/selectCodes", request, cancellationToken: cancellationToken);

            if (!httpResponse.IsSuccessStatusCode)
            {
                return Result.Fail(new Error($"Error response from ZRA API Fetch Standard Codes request. Status: {httpResponse.StatusCode} Reason: {httpResponse.ReasonPhrase}."));
            }

            var response = await httpResponse.Content.ReadFromJsonAsync<FetchStandardCodesResponse>(cancellationToken: cancellationToken);

            if (response is null)
            {
                string body = await httpResponse.Content.ReadAsStringAsync(cancellationToken);

                return Result.Fail(new Error($"Unexepected response content from ZRA API Fetch Standard Codes request. ResponseBody: {body}"));
            }

            var standardCodeClasses = response.clsList.ToDtos();

            return Result.Ok(standardCodeClasses);
        }
        catch (TimeoutRejectedException tEx)
        {
            return Result.Fail(new HttpRequestTimoutError("Zra API Fetch Standard Codes request timed-out.", tEx));
        }
        catch (BrokenCircuitException cEx)
        {
            return Result.Fail(new CircuitBreakerError("Zra API Fetch Standard Codes request failed due to the circuit breaker.", cEx));
        }
        catch (Exception ex)
        {
            return Result.Fail(new ExceptionalError(ex));
        }
    }

    public async Task<Result<ImmutableArray<ClassificationCodeDto>>> FetchClassificationCodes(CancellationToken cancellationToken)
    {
        try
        {
            _logger.LogInformation("Sending Fetch Classification Codes request to the ZRA API.");

            var request = new FetchClassificationCodesRequest
            {
                tpin = _apiOptions.TaxpayerIdentificationNumber,
                bhfId = _apiOptions.TaxpayerBranchIdentifier,
            };

            HttpResponseMessage httpResponse = await _httpClient.PostAsJsonAsync("itemClass/selectItemsClass", request, cancellationToken: cancellationToken);

            if (!httpResponse.IsSuccessStatusCode)
            {
                return Result.Fail(new Error($"Error response from ZRA API Fetch Classification Codes request. Status: {httpResponse.StatusCode} Reason: {httpResponse.ReasonPhrase}."));
            }

            var response = await httpResponse.Content.ReadFromJsonAsync<FetchClassificationCodesResponse>(cancellationToken: cancellationToken);

            if (response is null)
            {
                string body = await httpResponse.Content.ReadAsStringAsync(cancellationToken);

                return Result.Fail(new Error($"Unexepected response content from ZRA API Fetch Classification Codes request. ResponseBody: {body}"));
            }

            var dtos = response.itemClsList.ToDtos();

            return
                response.resultCd == ZraResponseCode.SUCCESS || response.resultCd == ZraResponseCode.NO_SEARCH_RESULT
                ? Result.Ok(dtos)
                : Result.Fail($"ZRA API Fetch Classification Codes request failure response recieved: Code: {response.resultCd}; Message: {response.resultMsg}.");
        }
        catch (TimeoutRejectedException tEx)
        {
            return Result.Fail(new HttpRequestTimoutError("ZRA API Fetch Classification Codes request timed-out.", tEx));
        }
        catch (BrokenCircuitException cEx)
        {
            return Result.Fail(new CircuitBreakerError("ZRA API Fetch Classification Codes request failed due to the circuit breaker.", cEx));
        }
        catch (Exception ex)
        {
            return Result.Fail(new ExceptionalError(ex));
        }
    }

    public async Task<Result> SaveItem(PluItemDto dto, CancellationToken cancellationToken)
    {
        try
        {
            _logger.LogInformation("Sending Save Item request to the ZRA API.");

            var request = dto.ToSaveItemRequest(_apiOptions);

            var httpResponse = await _httpClient.PostAsJsonAsync("items/saveItem", request, cancellationToken);

            if (!httpResponse.IsSuccessStatusCode)
            {
                return Result.Fail(new Error($"Error response from ZRA API Save Item request. Status: {httpResponse.StatusCode} Reason: {httpResponse.ReasonPhrase}."));
            }

            var response = await httpResponse.Content.ReadFromJsonAsync<SaveItemResponse>(cancellationToken: cancellationToken);

            if (response is null)
            {
                string body = await httpResponse.Content.ReadAsStringAsync(cancellationToken);

                return Result.Fail(new Error($"Unexepected response content from ZRA API Save Item request. ResponseBody: {body}"));
            }

            return
                response.resultCd == ZraResponseCode.SUCCESS
                ? Result.Ok()
                : Result.Fail($"ZRA API Save Item request failure response recieved: Code: {response.resultCd}; Message: {response.resultMsg}.");
        }
        catch (TimeoutRejectedException tEx)
        {
            return Result.Fail(new HttpRequestTimoutError("ZRA API Save Item request timed-out.", tEx));
        }
        catch (BrokenCircuitException cEx)
        {
            return Result.Fail(new CircuitBreakerError("ZRA API Save Item request failed due to the circuit breaker.", cEx));
        }
        catch (Exception ex)
        {
            return Result.Fail(new ExceptionalError(ex));
        }
    }

    public async Task<Result> UpdateItem(PluItemDto dto, CancellationToken cancellationToken)
    {
        try
        {
            _logger.LogInformation("Sending Update Item request to the ZRA API.");

            var request = dto.ToUpdateItemRequest(_apiOptions);

            var httpResponse = await _httpClient.PostAsJsonAsync("items/updateItem", request, cancellationToken);

            if (!httpResponse.IsSuccessStatusCode)
            {
                return Result.Fail(new Error($"Error response from ZRA API Update Item request. Status: {httpResponse.StatusCode} Reason: {httpResponse.ReasonPhrase}."));
            }

            var response = await httpResponse.Content.ReadFromJsonAsync<UpdateItemResponse>(cancellationToken: cancellationToken);

            if (response is null)
            {
                string body = await httpResponse.Content.ReadAsStringAsync(cancellationToken);

                return Result.Fail(new Error($"Unexepected response content from ZRA API Update Item request. ResponseBody: {body}"));
            }

            return
                response.resultCd == ZraResponseCode.SUCCESS
                ? Result.Ok()
                : Result.Fail($"ZRA API Update Item request failure response recieved: Code: {response.resultCd}; Message: {response.resultMsg}.");
        }
        catch (TimeoutRejectedException tEx)
        {
            return Result.Fail(new HttpRequestTimoutError("ZRA API Update Item request timed-out.", tEx));
        }
        catch (BrokenCircuitException cEx)
        {
            return Result.Fail(new CircuitBreakerError("ZRA API Update Item request failed due to the circuit breaker.", cEx));
        }
        catch (Exception ex)
        {
            return Result.Fail(new ExceptionalError(ex));
        }
    }

    public async Task<Result<ImmutableArray<ImportItemDto>>> GetImports(CancellationToken cancellationToken)
    {
        try
        {
            _logger.LogInformation("Sending Get Imports request to the ZRA API.");

            var request = new GetImportsRequest
            {
                tpin = _apiOptions.TaxpayerIdentificationNumber,
                bhfId = _apiOptions.TaxpayerBranchIdentifier,
            };

            var httpResponse = await _httpClient.PostAsJsonAsync("imports/selectImportItems", request, cancellationToken);

            if (!httpResponse.IsSuccessStatusCode)
            {
                return Result.Fail(new Error($"Error response from ZRA API Get Imports request. Status: {httpResponse.StatusCode} Reason: {httpResponse.ReasonPhrase}."));
            }

            var response = await httpResponse.Content.ReadFromJsonAsync<GetImportsResponse>(cancellationToken: cancellationToken);

            if (response is null)
            {
                string body = await httpResponse.Content.ReadAsStringAsync(cancellationToken);

                return Result.Fail(new Error($"Unexepected response content from ZRA API Get Imports request. ResponseBody: {body}"));
            }

            var dtos = response.data?.ToDtos() ?? [];

            return
                response.resultCd == ZraResponseCode.SUCCESS || response.resultCd == ZraResponseCode.NO_SEARCH_RESULT
                ? Result.Ok(dtos)
                : Result.Fail($"ZRA API Get Imports request failure response recieved: Code: {response.resultCd}; Message: {response.resultMsg}.");
        }
        catch (TimeoutRejectedException tEx)
        {
            return Result.Fail(new HttpRequestTimoutError("ZRA API Get Imports request timed-out.", tEx));
        }
        catch (BrokenCircuitException cEx)
        {
            return Result.Fail(new CircuitBreakerError("ZRA API Get Imports request failed due to the circuit breaker.", cEx));
        }
        catch (Exception ex)
        {
            return Result.Fail(new ExceptionalError(ex));
        }
    }

    public async Task<Result<ImmutableArray<PurchaseDto>>> GetPurchases(CancellationToken cancellationToken)
    {
        try
        {
            _logger.LogInformation("Sending Get Purchases request to the ZRA API.");

            var request = new GetPurchasesRequest
            {
                tpin = _apiOptions.TaxpayerIdentificationNumber,
                bhfId = _apiOptions.TaxpayerBranchIdentifier,
            };

            var httpResponse = await _httpClient.PostAsJsonAsync("trnsPurchase/selectTrnsPurchaseSales", request, cancellationToken);

            if (!httpResponse.IsSuccessStatusCode)
            {
                return Result.Fail(new Error($"Error response from ZRA API Get Purchases request. Status: {httpResponse.StatusCode} Reason: {httpResponse.ReasonPhrase}."));
            }

            var response = await httpResponse.Content.ReadFromJsonAsync<GetPurchasesResponse>(cancellationToken: cancellationToken);

            if (response is null)
            {
                string body = await httpResponse.Content.ReadAsStringAsync(cancellationToken);

                return Result.Fail(new Error($"Unexepected response content from ZRA API Get Purchases request. ResponseBody: {body}"));
            }

            var dtos = response.MapToDtos();

            return
                response.resultCd == ZraResponseCode.SUCCESS || response.resultCd == ZraResponseCode.NO_SEARCH_RESULT
                ? Result.Ok(dtos)
                : Result.Fail($"ZRA API Get Purchases request failure response recieved: Code: {response.resultCd}; Message: {response.resultMsg}.");
        }
        catch (TimeoutRejectedException tEx)
        {
            return Result.Fail(new HttpRequestTimoutError("ZRA API Get Purchases request timed-out.", tEx));
        }
        catch (BrokenCircuitException cEx)
        {
            return Result.Fail(new CircuitBreakerError("ZRA API Get Purchases request failed due to the circuit breaker.", cEx));
        }
        catch (Exception ex)
        {
            return Result.Fail(new ExceptionalError(ex));
        }
    }

    public async Task<Result> SavePurchase(PurchaseDto purchase, CancellationToken cancellationToken)
    {
        try
        {
            _logger.LogInformation("Sending Save Purchases request to the ZRA API.");

            var request = purchase.MapToSavePurchaseRequest(_apiOptions);

            var httpResponse = await _httpClient.PostAsJsonAsync("trnsPurchase/savePurchase", request, cancellationToken);

            if (!httpResponse.IsSuccessStatusCode)
            {
                return Result.Fail(new Error($"Error response from ZRA API Save Purchase request. Status: {httpResponse.StatusCode} Reason: {httpResponse.ReasonPhrase}."));
            }

            var response = await httpResponse.Content.ReadFromJsonAsync<GetPurchasesResponse>(cancellationToken: cancellationToken);

            if (response is null)
            {
                string body = await httpResponse.Content.ReadAsStringAsync(cancellationToken);

                return Result.Fail(new Error($"Unexepected response content from ZRA API Save Purchase request. ResponseBody: {body}"));
            }

            return
                response.resultCd == ZraResponseCode.SUCCESS
                ? Result.Ok()
                : Result.Fail($"ZRA API Save Purchase request failure response recieved: Code: {response.resultCd}; Message: {response.resultMsg}.");
        }
        catch (TimeoutRejectedException tEx)
        {
            return Result.Fail(new HttpRequestTimoutError("ZRA API Save Purchase request timed-out.", tEx));
        }
        catch (BrokenCircuitException cEx)
        {
            return Result.Fail(new CircuitBreakerError("ZRA API Save Purchase request failed due to the circuit breaker.", cEx));
        }
        catch (Exception ex)
        {
            return Result.Fail(new ExceptionalError(ex));
        }
    }
}
