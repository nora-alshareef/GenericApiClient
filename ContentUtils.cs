namespace GenericApiClient;

public enum ContentType
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
    public static string GetMimeType(this ContentType contentType)
    {
        return contentType switch
        {
            ContentType.ApplicationJson => "application/json",
            ContentType.FormUrlEncoded => "application/x-www-form-urlencoded",
            ContentType.TextPlain => "text/plain",
            ContentType.ApplicationXml => "application/xml",
            ContentType.ApplicationPdf => "application/pdf",
            ContentType.ImageJpeg => "image/jpeg",
            ContentType.ImagePng => "image/png",
            ContentType.ApplicationOctetStream => "application/octet-stream",
            ContentType.TextHtml => "text/html",
            ContentType.TextCsv => "text/csv",
            _ => throw new ArgumentOutOfRangeException(nameof(contentType), contentType, null)
        };
    }
}

public static class ContentSerializer
{
    internal static object SerializeBody(object body, ContentType contentType)
    {
        switch (contentType)
        {
            case ContentType.ApplicationJson:
                return System.Text.Json.JsonSerializer.Serialize(body);
            case ContentType.FormUrlEncoded:
                return SerializeFormUrlEncoded(body);
            case ContentType.TextPlain:
                return body?.ToString() ?? string.Empty;
            case ContentType.ImageJpeg:
            case ContentType.ImagePng:
            case ContentType.ApplicationOctetStream:
                 return SerializeBinaryData(body);
            default:
                throw new ArgumentException($"Unsupported content type: {contentType}");
        }
    }

    private static string SerializeFormUrlEncoded(object body)
    {
        if (body is Dictionary<string, string> dict)
        {
            return string.Join("&",
                dict.Select(kvp => $"{Uri.EscapeDataString(kvp.Key)}={Uri.EscapeDataString(kvp.Value)}"));
        }
        else if (body is IEnumerable<KeyValuePair<string, string>> kvpList)
        {
            return string.Join("&",
                kvpList.Select(kvp => $"{Uri.EscapeDataString(kvp.Key)}={Uri.EscapeDataString(kvp.Value)}"));
        }
        else
        {
            throw new ArgumentException(
                "For form-urlencoded, body must be Dictionary<string, string> or IEnumerable<KeyValuePair<string, string>>");
        }
    }
    
    private static byte[] SerializeBinaryData(object body)
    {
        if (body is byte[] byteArray)
        {
            return byteArray;
        }
        else if (body is Stream stream)
        {
            using (MemoryStream memoryStream = new MemoryStream())
            {
                stream.CopyTo(memoryStream);
                return memoryStream.ToArray();
            }
        }
        else if (body is string filePath && File.Exists(filePath))
        {
            return File.ReadAllBytes(filePath);
        }
        else
        {
            throw new ArgumentException("For binary data, body must be byte[], Stream, or a valid file path");
        }
    }
}

public static class ContentDeserializer
{
    internal static T DeserializeContent<T>(string content, ContentType contentType) where T : class
    {
        switch (contentType)
        {
            case ContentType.ApplicationJson:
                return System.Text.Json.JsonSerializer.Deserialize<T>(content);
            case ContentType.TextPlain:
                return content as T;
            case ContentType.FormUrlEncoded:
                return DeserializeFormUrlEncoded<T>(content);
            case ContentType.ImageJpeg:
            case ContentType.ImagePng:
            case ContentType.ApplicationOctetStream:
                return DeserializeBinaryData<T>(content);
            default:
                throw new ArgumentException($"Unsupported content type for deserialization: {contentType}");
        }
    }

    private static T DeserializeFormUrlEncoded<T>(string content) where T : class
    {
        var formData = System.Web.HttpUtility.ParseQueryString(content);
        var obj = Activator.CreateInstance<T>();
        foreach (var prop in typeof(T).GetProperties())
        {
            var value = formData[prop.Name];
            if (value != null)
            {
                prop.SetValue(obj, Convert.ChangeType(value, prop.PropertyType));
            }
        }
        return obj;
    }

    private static T DeserializeBinaryData<T>(string content) where T : class
    {
        // Assuming the content is a Base64 encoded string for binary data
        byte[] binaryData = Convert.FromBase64String(content);

        if (typeof(T) == typeof(byte[]))
        {
            return binaryData as T;
        }
        else if (typeof(T) == typeof(Stream))
        {
            return new MemoryStream(binaryData) as T;
        }
        else
        {
            throw new ArgumentException("For binary data, T must be byte[] or Stream");
        }
    }
}