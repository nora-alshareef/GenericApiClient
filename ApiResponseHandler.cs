using System.Net;

namespace GenericApiClient;

internal static class ApiResponseHandler
{
    private static readonly HttpClient _httpClient;

    static ApiResponseHandler()
    {
        var httpClientHandler = new HttpClientHandler
        {
            ServerCertificateCustomValidationCallback = (message, cert, chain, errors) => true
        };
        _httpClient = new HttpClient(httpClientHandler) { Timeout = TimeSpan.FromSeconds(30) };
    }

    public static async Task<ApiResponse<TSuccessResponse, TFailureResponse>> HandleRequestAsync<TSuccessResponse,
        TFailureResponse>(
        HttpRequestMessage request,
        MediaType acceptType)
        where TSuccessResponse : class
        where TFailureResponse : class
    {
        var response = new ApiResponse<TSuccessResponse, TFailureResponse>();

        try
        {
            using var httpResponse = await _httpClient.SendAsync(request);
            response.StatusCode = httpResponse.StatusCode;
            response.RawContent = await httpResponse.Content.ReadAsStringAsync();
            ProcessResponse(response, acceptType);
        }
        catch (Exception ex)
        {
            response.Exception = ex;
            response.ErrorMessage = $"Exception occurred: {ex.Message}";
        }

        return response;
    }


    private static void ProcessResponse<TSuccessResponse, TFailureResponse>(
        ApiResponse<TSuccessResponse, TFailureResponse> response,
        MediaType acceptType)
        where TSuccessResponse : class
        where TFailureResponse : class
    {
        if (response.IsSuccessful)
            ProcessSuccessfulResponse(response, acceptType);
        else if (response.StatusCode == HttpStatusCode.BadRequest && response.HasContent)
            response.FailureResponse =
                ContentDeserializer.DeserializeContent<TFailureResponse>(response.RawContent, acceptType);
        else
            response.ErrorMessage =
                $"Request failed with status code: {(int)response.StatusCode}:{response.StatusCode}";
    }

    private static void ProcessSuccessfulResponse<TSuccessResponse, TFailureResponse>(
        ApiResponse<TSuccessResponse, TFailureResponse> response,
        MediaType acceptType)
        where TSuccessResponse : class
        where TFailureResponse : class
    {
        if (response.HasContent)
            try
            {
                if (typeof(TSuccessResponse).IsClass)
                {
                    response.SuccessResponse =
                        ContentDeserializer.DeserializeContent<TSuccessResponse>(response.RawContent, acceptType);
                }else
                {
                    // If T is a primitive, cast it directly
                    response.SuccessResponse = (TSuccessResponse)Convert.ChangeType(response.RawContent, typeof(TSuccessResponse));
                }
            }
            catch (Exception) // This catches all potential exceptions
            {
                response.IsAmbiguous = true;
                response.SuccessResponse = null;
                response.ErrorMessage = $"Request is ambiguous, raw content: {response.RawContent}";
            }
    }
}