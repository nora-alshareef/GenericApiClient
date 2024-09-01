using System.Globalization;
using System.Net.Http.Headers;
using System.Text;
using System.Web;

namespace GenericApiClient
{
    internal static class ApiUtils
    {
        internal static UriBuilder BuildUri(string baseUrl, Dictionary<string, object>? queryParams)
        {
            if (string.IsNullOrWhiteSpace(baseUrl))
            {
                throw new ArgumentNullException(nameof(baseUrl), "Base URL cannot be null or empty.");
            }

            var uriBuilder = new UriBuilder(baseUrl);
            var query = HttpUtility.ParseQueryString(uriBuilder.Query);

            if (queryParams != null)
            {
                foreach (var param in queryParams)
                {
                    string? valueString = FormatQueryParamValue(param.Value);
                    query[param.Key] = valueString;
                }
            }

            uriBuilder.Query = query.ToString();
            return uriBuilder;
        }

        private static string? FormatQueryParamValue(object value)
        {
            return value switch
            {
                null => string.Empty,
                DateTime dateTime => dateTime.ToString("o"),
                DateTimeOffset dateTimeOffset => dateTimeOffset.ToString("o"),
                IFormattable formattable => formattable.ToString(null, CultureInfo.InvariantCulture),
                IEnumerable<string> stringList => string.Join(",", stringList),
                IEnumerable<int> intList => string.Join(",", intList),
                _ => value.ToString()
            };
        }

        internal static HttpRequestMessage CreateHttpRequest(
            HttpMethod method,
            Uri uri,
            string contentType,
            Dictionary<string, string>? headers = null)
        {
            var request = new HttpRequestMessage(method, uri);

            SetContentTypeHeader(request, method, contentType);

            // Add other headers if provided
            if (headers != null)
            {
                foreach (var header in headers)
                {
                    request.Headers.TryAddWithoutValidation(header.Key, header.Value);
                }
            }
            return request;
        }
        
        public static HttpContent CreateHttpContent(object serializedBody, string contentType)
        {
            switch (serializedBody)
            {
                case string stringContent:
                    return new StringContent(stringContent, Encoding.UTF8, contentType);
                case byte[] byteContent:
                {
                    var content = new ByteArrayContent(byteContent);
                    content.Headers.ContentType = new MediaTypeHeaderValue(contentType);
                    return content;
                }
                default:
                    throw new InvalidOperationException($"Unexpected serialized body type: {serializedBody?.GetType()}");
            }
        }

        private static void SetContentTypeHeader(HttpRequestMessage request, HttpMethod method, string contentType)
        {
            if (method == HttpMethod.Post || method == HttpMethod.Put || method == HttpMethod.Patch)
            {
                request.Content = new StringContent(string.Empty);
                request.Content.Headers.ContentType = new MediaTypeHeaderValue(contentType);
            }
            else if (method == HttpMethod.Get || method == HttpMethod.Delete || method == HttpMethod.Head)
            {
                request.Headers.TryAddWithoutValidation("Content-Type", contentType);
            }
        }
    }
}