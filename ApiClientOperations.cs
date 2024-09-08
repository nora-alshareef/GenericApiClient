namespace GenericApiClient;

public static class ApiClientOperations
{
    /// <summary>
    ///     Performs a generic GET request with specified content type and parameters.
    /// </summary>
    /// <typeparam name="TSuccessResponse">The type of the expected successful response.</typeparam>
    /// <typeparam name="TFailureResponse">The type of the expected failure response.</typeparam>
    /// <param name="baseUrl">The base URL of the API endpoint.</param>
    /// <param name="contentType">represents the type of data being sent in an HTTP request body</param>
    /// <param name="acceptType"> to specify the media types that are acceptable for the response from the server</param>
    /// <param name="queryParams">Optional query parameters to be appended to the URL.</param>
    /// <param name="headers">Optional additional headers for the request.</param>
    /// <returns>
    ///     A task that represents the asynchronous operation. The task result contains
    ///     an ApiResponse with either a successful response of type TSuccessResponse
    ///     or a failure response of type TFailureResponse.
    /// </returns>
    /// <remarks>
    ///     This method can handle various content types (e.g., JSON, Form, XML) for GET requests.
    ///     The Accept header is set to "*/*" by default, allowing any response type.
    /// </remarks>
    public static async Task<ApiResponse<TSuccessResponse, TFailureResponse>> GetAsync<TSuccessResponse,
        TFailureResponse>(
        string baseUrl,
        MediaType contentType,
        MediaType acceptType,
        Dictionary<string, object>? queryParams,
        Dictionary<string, string>? headers = null)
        where TSuccessResponse : class
        where TFailureResponse : class
    {
        var contentTypeStr = contentType.GetMimeType();
        //headers["Accept"] = "*/*";
        var uriBuilder = ApiUtils.BuildUri(baseUrl, queryParams);
        var request = ApiUtils.CreateHttpRequest(HttpMethod.Get, uriBuilder.Uri, contentTypeStr, headers);
        return await ApiResponseHandler.HandleRequestAsync<TSuccessResponse, TFailureResponse>(request,acceptType);
    }

    public static async Task<ApiResponse<TSuccessResponse, TFailureResponse>> PostAsync<TSuccessResponse,
        TFailureResponse>(
        string baseUrl,
        MediaType contentType,
        MediaType acceptType,
        Dictionary<string, string>? headers = null,
        Dictionary<string, object>? queryParams = null,
        object? body = null)
        where TSuccessResponse : class
        where TFailureResponse : class
    {
        // Validate essential parameters
        if (string.IsNullOrWhiteSpace(baseUrl))
            throw new ArgumentNullException(nameof(baseUrl), "Base URL must be provided for a POST request.");

        // It's okay if both queryParams and body are null for POST
        // Some APIs might accept empty POST requests
        var contentTypeStr = contentType.GetMimeType(); //already check if contentType is null =>exception
        // If headers are null, initialize an empty dictionary
        headers ??= new Dictionary<string, string>();

        var uriBuilder = ApiUtils.BuildUri(baseUrl, queryParams);
        var request = ApiUtils.CreateHttpRequest(HttpMethod.Post, uriBuilder.Uri, contentTypeStr, headers);

        if (body == null)
            return await ApiResponseHandler.HandleRequestAsync<TSuccessResponse, TFailureResponse>(request,
                acceptType);
        
        var serializedRequestBody = ContentSerializer.SerializeBody(body, contentType);
        request.Content = ApiUtils.CreateHttpContent(serializedRequestBody, contentTypeStr);

        return await ApiResponseHandler.HandleRequestAsync<TSuccessResponse, TFailureResponse>(request,acceptType);
    }


    public static async Task<ApiResponse<TSuccessResponse, TFailureResponse>> DeleteAsync<TSuccessResponse,
        TFailureResponse>(
        string baseUrl,
        MediaType contentType,
        MediaType acceptType,
        Dictionary<string, string>? headers = null,
        Dictionary<string, object>? queryParams = null
    )
        where TSuccessResponse : class
        where TFailureResponse : class
    {
        // Validate essential parameters
        if (string.IsNullOrWhiteSpace(baseUrl))
            throw new ArgumentNullException(nameof(baseUrl), "Base URL must be provided for a DELETE request.");

        // It's okay if headers are null
        headers ??= new Dictionary<string, string>();

        var contentTypeStr = contentType.GetMimeType(); // Ensure contentType is not null
        var uriBuilder = ApiUtils.BuildUri(baseUrl, queryParams);
        var request = ApiUtils.CreateHttpRequest(HttpMethod.Delete, uriBuilder.Uri, contentTypeStr, headers);

        return await ApiResponseHandler.HandleRequestAsync<TSuccessResponse, TFailureResponse>(request, acceptType);
    }
}