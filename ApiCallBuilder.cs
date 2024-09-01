using System.Net;

namespace GenericApiClient

{
    public class ApiCallBuilder<TSuccessResponse, TErrorResponse>
        where TSuccessResponse : class
        where TErrorResponse : class
    {
        private string? _endpoint;
        private HttpMethod? _method;
        private ContentType _contentType;
        private Dictionary<string, string>? _headers;
        private object? _body;
        private Dictionary<string, object>? _queryParams;

        private Dictionary<HttpStatusCode, Func<ApiResponse<TSuccessResponse, TErrorResponse>, Task<object>>>
            _statusHandlers = new();

        private Func<HttpStatusCode, string, Task<object>>? _defaultHandler;

        public ApiCallBuilder<TSuccessResponse, TErrorResponse> SetEndpoint(string endpoint)
        {
            _endpoint = endpoint;
            return this;
        }

        public ApiCallBuilder<TSuccessResponse, TErrorResponse> SetMethod(HttpMethod method)
        {
            _method = method;
            return this;
        }

        public ApiCallBuilder<TSuccessResponse, TErrorResponse> SetContentType(ContentType contentType)
        {
            _contentType = contentType;
            return this;
        }

        public ApiCallBuilder<TSuccessResponse, TErrorResponse> SetHeaders(Dictionary<string, string> headers)
        {
            _headers = headers;
            return this;
        }

        public ApiCallBuilder<TSuccessResponse, TErrorResponse> AddHeader(string key, string value)
        {
            _headers ??= new Dictionary<string, string>();
            _headers[key] = value;
            return this;
        }

        public ApiCallBuilder<TSuccessResponse, TErrorResponse> SetBody(object body)
        {
            _body = body;
            return this;
        }

        public ApiCallBuilder<TSuccessResponse, TErrorResponse> SetQueryParams(Dictionary<string, object> queryParams)
        {
            _queryParams = queryParams;
            return this;
        }

        public ApiCallBuilder<TSuccessResponse, TErrorResponse> AddQueryParam(string key, object value)
        {
            _queryParams ??= new Dictionary<string, object>();
            _queryParams[key] = value;
            return this;
        }

        public ApiCallBuilder<TSuccessResponse, TErrorResponse> AddStatusHandler<T>(
            HttpStatusCode statusCode,
            Func<ApiResponse<TSuccessResponse, TErrorResponse>, Task<T>> handler)
        {
            _statusHandlers[statusCode] = async response => await handler(response);
            return this;
        }

        public ApiCallBuilder<TSuccessResponse, TErrorResponse> SetDefaultHandler<T>(
            Func<HttpStatusCode, string, Task<T>> handler)
        {
            _defaultHandler = async (statusCode, errorMessage) => await handler(statusCode, errorMessage);
            return this;
        }

        public async Task<T> ExecuteAsync<T>()
        {
            if (string.IsNullOrEmpty(_endpoint))
                throw new InvalidOperationException("Endpoint must be set before executing the API call.");

            if (_method == null)
                throw new InvalidOperationException("HTTP method must be set before executing the API call.");

            ApiResponse<TSuccessResponse, TErrorResponse> response;

            if (_method == HttpMethod.Get)
                response = await ApiClientOperations.GetAsync<TSuccessResponse, TErrorResponse>(_endpoint, _contentType,
                    _queryParams, _headers);
            else if (_method == HttpMethod.Post)
                response = await ApiClientOperations.PostAsync<TSuccessResponse, TErrorResponse>(_endpoint,
                    _contentType,
                    _headers, _queryParams, _body);
            else
                throw new NotSupportedException($"HTTP method {_method} is not supported.");

            if (_statusHandlers.TryGetValue(response.StatusCode, out var handler))
            {
                var result = await handler(response);
                if (result is T typedResult)
                    return typedResult;
                throw new InvalidOperationException(
                    $"Handler returned unexpected type. Expected {typeof(T)}, got {result?.GetType()}");
            }
            else if (_defaultHandler != null)
            {
                var result = await _defaultHandler(response.StatusCode, response.ErrorMessage ?? string.Empty);
                if (result is T typedResult)
                    return typedResult;
                throw new InvalidOperationException(
                    $"Default handler returned unexpected type. Expected {typeof(T)}, got {result?.GetType()}");
            }
            else
            {
                throw new HttpRequestException(
                    $"Unhandled HTTP status code: {response.StatusCode}, Content: {response.RawContent}");
            }
        }
    }
}
