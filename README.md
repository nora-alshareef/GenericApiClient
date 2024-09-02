# GenericApiClient

GenericApiClient is a flexible and easy-to-use C# library for making HTTP API calls. It provides a simple interface for sending GET and POST requests, handling responses, and managing content types.

## Installation
### Build and Use the Library

1. Clone and build the repository:

```shell
   git clone https://github.com/nora-alshareef/GenericApiClient
   cd GenericApiClient
   dotnet build
   dotnet pack
   vhdl
```
   

### Add Reference to the Generic API Client DLL

To add a reference to the Generic API Client DLL in your project, follow these steps:

1. Right-click on your project in the Solution Explorer.
2. Select "Add" > "Reference".
3. In the Reference Manager dialog, click on "Browse".
4. Navigate to the location of the Generic API Client DLL file.
5. Select the DLL file (e.g., "GenericApiClient.dll") and click "Add".
6. Click "OK" in the Reference Manager dialog to confirm.

Alternatively, if you're using the command line or prefer editing the .csproj file directly, you can add the following line within an `<ItemGroup>` in your project file:

```xml
<Reference Include="GenericApiClient">
 <HintPath>path\to\GenericApiClient.dll</HintPath>
</Reference>
```
## Usage

### Using ApiClientOperations

The `ApiClientOperations` class is the main entry point for making API calls. It provides methods for GET and POST requests.

#### GET Request

```csharp
var response = await ApiClientOperations.GetAsync<SuccessResponse, ErrorResponse>(
    endpoint,
    ContentType.FormUrlEncoded,
    queryParams,
    null);
```

#### POST Request

```csharp
var response = await ApiClientOperations.PostAsync<SuccessResponse, ErrorResponse>(
    endpoint,
    ContentType.ApplicationJson,
    null,
    bodyContent);
```

### ApiResponse Structure

The `ApiResponse` object returned by these methods contains:

- `StatusCode`: The HTTP status code of the response
- `IsSuccessful`: A boolean indicating if the request was successful
- `SuccessResponse`: The deserialized success response (when applicable)
- `ErrorResponse`: The deserialized error response (if applicable)
- `FailureResponse`: The deserialized failure response (for bad requests only)
- `RawContent`: The raw string content of the response
- `Headers`: The response headers
- `IsAmbiguous`: Flags potentially ambiguous responses
- `ErrorMessage`: Contains any error message associated with the response that is not a bad request or server error
- `Exception`: Holds any exception that occurred during the API call for server errors

### Using ApiCallBuilder (Recommended)

While `ApiClientOperations` provides a straightforward way to make API calls, `ApiCallBuilder` offers more flexibility in handling responses based on their status.

Example usage:

```csharp
 var queryParams = new Dictionary<string, object>
    {
        { "Category", "some category value" },
        { "InStock", true },
    };
var builder = new ApiCallBuilder<List<Product>, ProductsErrorResponse>()()
        .SetEndpoint("https://api.example.com/products")
        .SetMethod(HttpMethod.Get)
        .SetContentType(ContentType.ApplicationJson)
        .AddHeader("Authorization", $"Bearer {_config.ApiKey}")
        .AddQueryParams(queryParams)
        .AddStatusHandler(HttpStatusCode.OK, async response =>
 {
 var productsCount = response.SuccessResponse?.Count ?? 0;
     logger.LogInformation("[Products] Successfully fetched {@Count} products", productsCount);
 return response.SuccessResponse ?? new List<GetAllPersonsResponse>();
 })
.AddStatusHandler(HttpStatusCode.BadRequest, async response =>
 { 
     //log and return empty list ..
 })
 .AddStatusHandler(
     specify another http status code and implement some logic to handle it.
 )

public class Product
{
    public int Id { get; set; }
    public string Name { get; set; }
    public decimal Price { get; set; }
    public string Category { get; set; }
    public bool InStock { get; set; }
}

public class ProductsErrorResponse
{
    public string Message { get; set; }
    public string ErrorCode { get; set; }
}
```

The `ApiCallBuilder` allows you to chain methods to configure your API call, and then execute it with full control over how you handle the response.

## Features

- Support for various content types (JSON, Form URL Encoded, etc.)
- Automatic serialization and deserialization of request/response bodies
- Flexible response handling with typed success and error responses
- Query parameter support
- Custom header support
