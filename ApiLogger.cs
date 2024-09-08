namespace GenericApiClient;

public static class ApiLogger
{
    public static void LogInfo(string message)
    {
        // Implement your logging logic here
        Console.WriteLine($"INFO: {message}");
    }

    public static void LogWarning(string message)
    {
        // Implement your logging logic here
        Console.WriteLine($"WARNING: {message}");
    }

    public static void LogError(string message)
    {
        // Implement your logging logic here
        Console.WriteLine($"ERROR: {message}");
    }

    public static void LogError(Exception ex, string message)
    {
        // Implement your logging logic here
        Console.WriteLine($"ERROR: {message}. Exception: {ex}");
    }

    public static void LogApiResponse<TSuccessResponse, TFailureResponse>(
        ApiResponse<TSuccessResponse, TFailureResponse> response,
        string callerClassName,
        string callerMethodName)
    {
        if (response.IsSuccessful)
            LogSuccessfulResponse(response, callerClassName, callerMethodName);
        else
            LogFailedResponse(response, callerClassName, callerMethodName);
    }

    private static void LogSuccessfulResponse<TSuccessResponse, TFailureResponse>(
        ApiResponse<TSuccessResponse, TFailureResponse> response,
        string callerClassName,
        string callerMethodName)
    {
        if (response.IsAmbiguous)
            LogWarning(
                $"[{callerClassName}][{callerMethodName}] Request rejected: {response.RawContent}");
        else
            LogInfo(
                $"[{callerClassName}][{callerMethodName}] Request successful. Status code: {response.StatusCode}");
    }

    private static void LogFailedResponse<TSuccessResponse, TFailureResponse>(
        ApiResponse<TSuccessResponse, TFailureResponse> response,
        string callerClassName,
        string callerMethodName)
    {
        if (response.Exception != null)
            LogError(response.Exception,
                $"[{callerClassName}][{callerMethodName}] Exception occurred: {response.ErrorMessage}");
        else
            LogError(
                $"[{callerClassName}][{callerMethodName}] Request failed. Status code: {response.StatusCode}. Error: {response.ErrorMessage}");
    }
}