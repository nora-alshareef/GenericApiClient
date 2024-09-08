using System.Text.Json;
using System.Web;

namespace GenericApiClient;

public enum MediaType
{
    ApplicationJson,
    FormUrlEncoded,
    TextPlain,
    ApplicationXml,
    ApplicationPdf,
    ImageJpeg,
    ImagePng,
    ApplicationOctetStream,
    TextHtml,
    TextCsv
}





internal static class ContentTypeExtensions
{
    public static string GetMimeType(this MediaType contentType)
    {
        return contentType switch
        {
            MediaType.ApplicationJson => "application/json",
            MediaType.FormUrlEncoded => "application/x-www-form-urlencoded",
            MediaType.TextPlain => "text/plain",
            MediaType.ApplicationXml => "application/xml",
            MediaType.ApplicationPdf => "application/pdf",
            MediaType.ImageJpeg => "image/jpeg",
            MediaType.ImagePng => "image/png",
            MediaType.ApplicationOctetStream => "application/octet-stream",
            MediaType.TextHtml => "text/html",
            MediaType.TextCsv => "text/csv",
            _ => throw new ArgumentOutOfRangeException(nameof(contentType), contentType, null)
        };
    }
}

public static class ContentSerializer
{
    internal static object SerializeBody(object body, MediaType contentType)
    {
        switch (contentType)
        {
            case MediaType.ApplicationJson:
                return JsonSerializer.Serialize(body);
            case MediaType.FormUrlEncoded:
                return SerializeFormUrlEncoded(body);
            case MediaType.TextPlain:
                return body?.ToString() ?? string.Empty;
            case MediaType.ImageJpeg:
            case MediaType.ImagePng:
            case MediaType.ApplicationOctetStream:
                return SerializeBinaryData(body);
            default:
                throw new ArgumentException($"Unsupported content type: {contentType}");
        }
    }

    private static string SerializeFormUrlEncoded(object body)
    {
        if (body is Dictionary<string, string> dict)
            return string.Join("&",
                dict.Select(kvp => $"{Uri.EscapeDataString(kvp.Key)}={Uri.EscapeDataString(kvp.Value)}"));
        if (body is IEnumerable<KeyValuePair<string, string>> kvpList)
            return string.Join("&",
                kvpList.Select(kvp => $"{Uri.EscapeDataString(kvp.Key)}={Uri.EscapeDataString(kvp.Value)}"));
        throw new ArgumentException(
            "For form-urlencoded, body must be Dictionary<string, string> or IEnumerable<KeyValuePair<string, string>>");
    }

    private static byte[] SerializeBinaryData(object body)
    {
        if (body is byte[] byteArray)
            return byteArray;
        if (body is Stream stream)
            using (var memoryStream = new MemoryStream())
            {
                stream.CopyTo(memoryStream);
                return memoryStream.ToArray();
            }

        if (body is string filePath && File.Exists(filePath))
            return File.ReadAllBytes(filePath);
        throw new ArgumentException("For binary data, body must be byte[], Stream, or a valid file path");
    }
}

public static class ContentDeserializer
{
    internal static T DeserializeContent<T>(string content, MediaType contentType) where T : class
    {
        switch (contentType)
        {
            case MediaType.ApplicationJson:
                return JsonSerializer.Deserialize<T>(content);
            case MediaType.TextPlain:
                return content as T;
            case MediaType.FormUrlEncoded:
                return DeserializeFormUrlEncoded<T>(content);
            case MediaType.ImageJpeg:
            case MediaType.ImagePng:
            case MediaType.ApplicationOctetStream:
                return DeserializeBinaryData<T>(content);
            default:
                throw new ArgumentException($"Unsupported content type for deserialization: {contentType}");
        }
    }

    private static T DeserializeFormUrlEncoded<T>(string content) where T : class
    {
        var formData = HttpUtility.ParseQueryString(content);
        var obj = Activator.CreateInstance<T>();
        foreach (var prop in typeof(T).GetProperties())
        {
            var value = formData[prop.Name];
            if (value != null) prop.SetValue(obj, Convert.ChangeType(value, prop.PropertyType));
        }

        return obj;
    }

    private static T DeserializeBinaryData<T>(string content) where T : class
    {
        // Assuming the content is a Base64 encoded string for binary data
        var binaryData = Convert.FromBase64String(content);

        if (typeof(T) == typeof(byte[]))
            return binaryData as T;
        if (typeof(T) == typeof(Stream))
            return new MemoryStream(binaryData) as T;
        throw new ArgumentException("For binary data, T must be byte[] or Stream");
    }
}