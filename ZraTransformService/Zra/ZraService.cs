using FluentResults;
using Microsoft.Extensions.Options;
using PowrIntegration.Dtos;
using PowrIntegration.Options;
using PowrIntegration.Zra.ClassificationCodes;
using PowrIntegration.Zra.GetImports;
using PowrIntegration.Zra.InitializeDevice;
using PowrIntegration.Zra.SaveItem;
using PowrIntegration.Zra.StandardCodes;
using PowrIntegration.Zra.UpdateItem;
using System.Collections.Immutable;
using System.Net.Http.Json;

namespace PowrIntegration.Zra;

public class ZraService(HttpClient httpClient, IOptions<ApiOptions> apiOptions)
{
    private readonly HttpClient _httpClient = httpClient;
    private readonly ApiOptions _apiOptions = apiOptions.Value;

    public async Task<Result<InitializationResponse>> InitializeDevice(CancellationToken cancellationToken)
    {
        try
        {
            var request = new InitializeRequest
            {
                tpin = apiOptions.Value.TaxpayerIdentificationNumber,
                bhfId = apiOptions.Value.TaxpayerBranchIdentifier,
                dvcSrlNo = apiOptions.Value.DeviceSerialNumber
            };

            HttpResponseMessage httpResponse = await _httpClient.PostAsJsonAsync("initializer/selectInitInfo", request, cancellationToken: cancellationToken);

            if (!httpResponse.IsSuccessStatusCode)
            {
                return Result.Fail(new Error($"Error response from Device Initialize call - Status: {httpResponse.StatusCode} Reason: {httpResponse.ReasonPhrase}."));
            }

            var response = await httpResponse.Content.ReadFromJsonAsync<InitializationResponse>(cancellationToken: cancellationToken);

            if (response is null)
            {
                string body = await httpResponse.Content.ReadAsStringAsync(cancellationToken);

                return Result.Fail(new Error($"Unexepected response content from Initialize Device call. ResponseBody: {body}"));
            }

            return Result.Ok(response);
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
            var request = new FetchStandardCodesRequest
            {
                tpin = _apiOptions.TaxpayerIdentificationNumber,
                bhfId = _apiOptions.TaxpayerBranchIdentifier,
            };

            HttpResponseMessage httpResponse = await _httpClient.PostAsJsonAsync("code/selectCodes", request, cancellationToken: cancellationToken);

            if (!httpResponse.IsSuccessStatusCode)
            {
                return Result.Fail(new Error($"Error response from Fetch Standard Codes call - Status: {httpResponse.StatusCode} Reason: {httpResponse.ReasonPhrase}."));
            }

            var response = await httpResponse.Content.ReadFromJsonAsync<FetchStandardCodesResponse>(cancellationToken: cancellationToken);

            if (response is null)
            {
                string body = await httpResponse.Content.ReadAsStringAsync(cancellationToken);

                return Result.Fail(new Error($"Unexepected response content from Fetch Standard Codes call. ResponseBody: {body}"));
            }

            var standardCodeClasses = response.clsList.MapToDtos();

            return Result.Ok(standardCodeClasses);
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
            var request = new FetchClassificationCodesRequest
            {
                tpin = _apiOptions.TaxpayerIdentificationNumber,
                bhfId = _apiOptions.TaxpayerBranchIdentifier,
            };

            HttpResponseMessage httpResponse = await _httpClient.PostAsJsonAsync("itemClass/selectItemsClass", request, cancellationToken: cancellationToken);

            if (!httpResponse.IsSuccessStatusCode)
            {
                return Result.Fail(new Error($"Error response from Fetch Classification Codes call - Status: {httpResponse.StatusCode} Reason: {httpResponse.ReasonPhrase}."));
            }

            var response = await httpResponse.Content.ReadFromJsonAsync<FetchClassificationCodesResponse>(cancellationToken: cancellationToken);

            if (response is null)
            {
                string body = await httpResponse.Content.ReadAsStringAsync(cancellationToken);

                return Result.Fail(new Error($"Unexepected response content from Fetch Classification Codes call. ResponseBody: {body}"));
            }

            var dtos = response.itemClsList.MapToDtos();

            return
                response.resultCd == ZraResponseCode.SUCCESS || response.resultCd == ZraResponseCode.NO_SEARCH_RESULT
                ? Result.Ok(dtos)
                : Result.Fail($"Fetch Classification Codes call failure response recieved: Code: {response.resultCd}; Message: {response.resultMsg}.");
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
            var request = dto.MapToSaveItemRequest(_apiOptions);

            var httpResponse = await _httpClient.PostAsJsonAsync("items/saveItem", request, cancellationToken);

            if (!httpResponse.IsSuccessStatusCode)
            {
                return Result.Fail(new Error($"Error response from Save Item call - Status: {httpResponse.StatusCode} Reason: {httpResponse.ReasonPhrase}."));
            }

            var response = await httpResponse.Content.ReadFromJsonAsync<SaveItemResponse>(cancellationToken: cancellationToken);

            if (response is null)
            {
                string body = await httpResponse.Content.ReadAsStringAsync(cancellationToken);

                return Result.Fail(new Error($"Unexepected response content from Save Item call. ResponseBody: {body}"));
            }

            return
                response.resultCd == ZraResponseCode.SUCCESS
                ? Result.Ok()
                : Result.Fail($"Save Item call failure response recieved: Code: {response.resultCd}; Message: {response.resultMsg}.");
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
            var request = dto.MapToUpdateItemRequest(_apiOptions);

            var httpResponse = await _httpClient.PostAsJsonAsync("items/updateItem", request, cancellationToken);

            if (!httpResponse.IsSuccessStatusCode)
            {
                return Result.Fail(new Error($"Error response from Update Item call - Status: {httpResponse.StatusCode} Reason: {httpResponse.ReasonPhrase}."));
            }

            var response = await httpResponse.Content.ReadFromJsonAsync<UpdateItemResponse>(cancellationToken: cancellationToken);

            if (response is null)
            {
                string body = await httpResponse.Content.ReadAsStringAsync(cancellationToken);

                return Result.Fail(new Error($"Unexepected response content from Update Item call. ResponseBody: {body}"));
            }

            return
                response.resultCd == ZraResponseCode.SUCCESS
                ? Result.Ok()
                : Result.Fail($"Update Item call failure response recieved: Code: {response.resultCd}; Message: {response.resultMsg}.");
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
            var request = new GetImportsRequest
            {
                tpin = _apiOptions.TaxpayerIdentificationNumber,
                bhfId = _apiOptions.TaxpayerBranchIdentifier,
            };

            var httpResponse = await _httpClient.PostAsJsonAsync("imports/selectImportItems", request, cancellationToken);

            if (!httpResponse.IsSuccessStatusCode)
            {
                return Result.Fail(new Error($"Error response from Get Imports call - Status: {httpResponse.StatusCode} Reason: {httpResponse.ReasonPhrase}."));
            }

            var response = await httpResponse.Content.ReadFromJsonAsync<GetImportsResponse>(cancellationToken: cancellationToken);

            if (response is null)
            {
                string body = await httpResponse.Content.ReadAsStringAsync(cancellationToken);

                return Result.Fail(new Error($"Unexepected response content from Get Imports call. ResponseBody: {body}"));
            }

            var dtos = response.data.MapToDtos();

            return
                response.resultCd == ZraResponseCode.SUCCESS || response.resultCd == ZraResponseCode.NO_SEARCH_RESULT
                ? Result.Ok()
                : Result.Fail($"Get Imports call failure response recieved: Code: {response.resultCd}; Message: {response.resultMsg}.");
        }
        catch (Exception ex)
        {
            return Result.Fail(new ExceptionalError(ex));
        }
    }
}
