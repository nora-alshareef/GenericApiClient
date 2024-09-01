using System.Net.Http.Headers;

namespace GenericApiClient
{
    public static class ApiClientOperations
    {
        /// <summary>
        /// Performs a generic GET request with specified content type and parameters.
        /// </summary>
        /// <typeparam name="TSuccessResponse">The type of the expected successful response.</typeparam>
        /// <typeparam name="TFailureResponse">The type of the expected failure response.</typeparam>
        /// <param name="baseUrl">The base URL of the API endpoint.</param>
        /// <param name="contentType">The content type of the request.</param>
        /// <param name="queryParams">Optional query parameters to be appended to the URL.</param>
        /// <param name="headers">Optional additional headers for the request.</param>
        /// <returns>
        /// A task that represents the asynchronous operation. The task result contains 
        /// an ApiResponse with either a successful response of type TSuccessResponse 
        /// or a failure response of type TFailureResponse.
        /// </returns>
        /// <remarks>
        /// This method can handle various content types (e.g., JSON, Form, XML) for GET requests.
        /// The Accept header is set to "*/*" by default, allowing any response type.
        /// </remarks>
        public static async Task<ApiResponse<TSuccessResponse, TFailureResponse>> GetAsync<TSuccessResponse, TFailureResponse>(
            string baseUrl,
            ContentType contentType,
            Dictionary<string, object>? queryParams,
            Dictionary<string, string>? headers = null)
            where TSuccessResponse : class
            where TFailureResponse : class
        {
            var contentTypeStr = contentType.GetMimeType();
            //headers["Accept"] = "*/*";
            var uriBuilder = ApiUtils.BuildUri(baseUrl, queryParams);
            var request = ApiUtils.CreateHttpRequest(HttpMethod.Get, uriBuilder.Uri, contentTypeStr, headers);
            return await ApiResponseHandler.HandleRequestAsync<TSuccessResponse, TFailureResponse>(request,contentType);
        }
        
        public static async Task<ApiResponse<TSuccessResponse, TFailureResponse>> PostAsync<TSuccessResponse, TFailureResponse>(
            string baseUrl,
            ContentType contentType,
            Dictionary<string, string>? headers = null,
            Dictionary<string, object>? queryParams = null,
            object? body = null)
            where TSuccessResponse : class
            where TFailureResponse : class
        {
            // Validate essential parameters
            if (string.IsNullOrWhiteSpace(baseUrl))
            {
                throw new ArgumentNullException(nameof(baseUrl), "Base URL must be provided for a POST request.");
            }

            // It's okay if both queryParams and body are null for POST
            // Some APIs might accept empty POST requests
            var contentTypeStr = contentType.GetMimeType(); //already check if contentType is null =>exception
            // If headers are null, initialize an empty dictionary
            headers ??= new Dictionary<string, string>();

            // Set the Accept header only if it's not already present
            headers.TryAdd("Accept", "*/*");

            var uriBuilder = ApiUtils.BuildUri(baseUrl, queryParams);
            var request = ApiUtils.CreateHttpRequest(HttpMethod.Post, uriBuilder.Uri, contentTypeStr, headers);

            if (body == null)
                return await ApiResponseHandler.HandleRequestAsync<TSuccessResponse, TFailureResponse>(request,
                    contentType);
            var serializedBody = ContentSerializer.SerializeBody(body, contentType);
            request.Content = ApiUtils.CreateHttpContent(serializedBody, contentTypeStr);

            return await ApiResponseHandler.HandleRequestAsync<TSuccessResponse, TFailureResponse>(request, contentType);
        }
        
    }
}