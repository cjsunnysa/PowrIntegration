using FluentResults;
using System.Net.Http.Json;
using ZraShared.Zra;
using ZraSyncService.Zra.ClassificationCodes;
using ZraSyncService.Zra.InitializeDevice;
using ZraSyncService.Zra.StandardCodes;

namespace ZraSyncService.Zra;

public class ZraService(HttpClient httpClient, IConfiguration configuration)
{
    private readonly HttpClient _httpClient = httpClient;
    private readonly IConfiguration _configuration = configuration;

    public async Task<Result<InitializationResponse>> InitializeDevice(CancellationToken cancellationToken)
    {
        try
        {
            var request = new InitializeRequest
            {
                tpin = _configuration.GetValue<string>("ZraApi:TaxpayerIdentificationNumber")!,
                bhfId = _configuration.GetValue<string>("ZraApi:TaxpayerBranchIdentifier")!,
                dvcSrlNo = _configuration.GetValue<string>("ZraApi:DeviceSerialNumber")!
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

    public async Task<Result<FetchStandardCodesResponse>> FetchStandardCodes(CancellationToken cancellationToken)
    {
        try
        {
            var request = new FetchStandardCodesRequest
            {
                tpin = _configuration.GetValue<string>("ZraApi:TaxpayerIdentificationNumber")!,
                bhfId = _configuration.GetValue<string>("ZraApi:TaxpayerBranchIdentifier")!,
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

            return Result.Ok(response);
        }
        catch (Exception ex)
        {
            return Result.Fail(new ExceptionalError(ex));
        }
    }

    public async Task<Result<FetchClassificationCodesResponse>> FetchClassificationCodes(CancellationToken cancellationToken)
    {
        try
        {
            var request = new FetchClassificationCodesRequest
            {
                tpin = _configuration.GetValue<string>("ZraApi:TaxpayerIdentificationNumber")!,
                bhfId = _configuration.GetValue<string>("ZraApi:TaxpayerBranchIdentifier")!,
            };

            HttpResponseMessage httpResponse = await _httpClient.PostAsJsonAsync("itemClass/selectItemsClass", request, cancellationToken: cancellationToken);

            if (!httpResponse.IsSuccessStatusCode)
            {
                return Result.Fail(new Error($"Error response from Fetch Standard Codes call - Status: {httpResponse.StatusCode} Reason: {httpResponse.ReasonPhrase}."));
            }

            var response = await httpResponse.Content.ReadFromJsonAsync<FetchClassificationCodesResponse>(cancellationToken: cancellationToken);

            if (response is null)
            {
                string body = await httpResponse.Content.ReadAsStringAsync(cancellationToken);

                return Result.Fail(new Error($"Unexepected response content from Fetch Standard Codes call. ResponseBody: {body}"));
            }

            return Result.Ok(response);
        }
        catch (Exception ex)
        {
            return Result.Fail(new ExceptionalError(ex));
        }
    }

    public async Task<Result> SaveItem(SaveItemRequest request, CancellationToken cancellationToken)
    {
        try
        {
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
}
