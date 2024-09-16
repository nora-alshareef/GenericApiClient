# GenericApiClient

ApiClient is a flexible and easy-to-use C# library for making HTTP API calls. It provides a simple interface for
sending GET and POST requests, handling responses, and managing content types.

## Installation

### Build and Use the Library

1. Clone and build the repository:

```shell
   git clone https://github.com/nora-alshareef/GenericApiClient
   cd GenericApiClient
   dotnet build
   dotnet pack
```

### Add Reference to the Generic API Client DLL

To add a reference to the Generic API Client DLL in your project, follow these steps:

1. Right-click on your project in the Solution Explorer.
2. Select "Add" > "Reference".
3. In the Reference Manager dialog, click on "Browse".
4. Navigate to the location of the Generic API Client DLL file.
5. Select the DLL file (e.g., "GenericApiClient.dll") and click "Add".
6. Click "OK" in the Reference Manager dialog to confirm.

Alternatively, if you're using the command line or prefer editing the .csproj file directly, you can add the following
line within an `<ItemGroup>` in your project file:

```xml
<Reference Include="GenericApiClient">
 <HintPath>path\to\GenericApiClient.dll</HintPath>
</Reference>
```

## Usage

### Using ApiCallBuilder (Recommended)

`ApiCallBuilder` offers flexibility in handling responses based on their status.

This builder pattern allows you to customize your API calls by adding various components such as endpoint, method,
headers, content type, query parameters, body, and status handlers.

Example usage:

```csharp

var builder = new ApiCallBuilder<List<Product>, ProductsErrorResponse>()()
        .SetEndpoint("https://api.example.com/products")
        .SetMethod(HttpMethod.Get)
        .SetContentType(MediaType.ApplicationJson)
        .SetAcceptType(MediaType.ApplicationJson)
        .AddHeader("Authorization", $"Bearer {_config.ApiKey}")
        .AddHeader("headerKey", "headerValue")
        .AddQueryParam(queryKey1, queryValue1)
        .AddQueryParam(queryKey2, queryValue2)
        .AddStatusHandler(HttpStatusCode.OK, async response =>
 {
 var productsCount = response.SuccessResponse?.Count ?? 0;
     logger.LogInformation("[Products] Successfully fetched {@Count} products", productsCount);
 return response.SuccessResponse ?? new List<Product>();
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

The `ApiCallBuilder` allows you to chain methods to configure your API call, and then execute it with full control over
how you handle the response.

### Using ApiClientOperations (Alternative approach)

The `ApiClientOperations` class serves as the primary interface for making API calls, providing methods for both GET and
POST requests. Before using it, ensure you have prepared the following:

- **Success Response**: Test the API using a tool like cURL to verify the success scenario and understand the expected
  response structure.
- **Failure Response**: Similarly, check how the API behaves in failure scenarios.
- **Endpoint**: Know the specific API endpoint you will be calling.
- **Headers**: Determine any required headers, such as authentication tokens.
- **Query Parameters**: Identify any parameters needed in the URL.
- **Request Body**: Prepare the body content if required by the POST request.
- **Content Type**: Specify the appropriate content type (e.g., JSON, form data).
- **Accept Type**: Specify the appropriate Accept header (e.g., JSON, form data).

Here are the method signatures for the GET and POST requests:

#### GET Method Signature

```csharp
public static async Task<ApiResponse<TSuccessResponse, TFailureResponse>> GetAsync<TSuccessResponse, TFailureResponse>(
    string baseUrl,
    MediaType ContentType,
    Dictionary<string, object>? queryParams,
    Dictionary<string, string>? headers = null)
```

#### POST Method Signature

```csharp
public static async Task<ApiResponse<TSuccessResponse, TFailureResponse>> PostAsync<TSuccessResponse, TFailureResponse>(
    string baseUrl,
    MediaType ContentType,
    Dictionary<string, string>? headers = null,
    Dictionary<string, object>? queryParams = null,
    object? body = null)
```
#### DELETE Method Signature

```csharp
    public static async Task<ApiResponse<TSuccessResponse, TFailureResponse>> DeleteAsync<TSuccessResponse,TFailureResponse>(
        string baseUrl,
        MediaType contentType,
        MediaType acceptType,
        Dictionary<string, string>? headers = null,
        Dictionary<string, object>? queryParams = null
    )
```
#### Example GET Request

```csharp
var response = await ApiClientOperations.GetAsync<SuccessResponse, ErrorResponse>(
    endpoint,
    MediaType.FormUrlEncoded,
    queryParams,
    null);
```

#### Example POST Request

```csharp
var response = await ApiClientOperations.PostAsync<SuccessResponse, ErrorResponse>(
    endpoint,
    MediaType.ApplicationJson,
    null,
    null,
    bodyContent);
```

#### Example DELETE Request

```csharp
var response = await ApiClientOperations.DeleteAsync<SuccessResponse, ErrorResponse>(
    endpoint,
    MediaType.ApplicationJson,
    null,
    queryParams);
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



## Example Usage

In the following example, we will create a simple application that utilizes the  `GenericApiClient`  package. This example includes the following components:

-   **ProductController**  with methods for  `POST`,  `GET`, and  `DELETE`.
-   **ProductService**  that leverages the  `ApiCallBuilder`  class.
-   **Product**  model representing a product entity.

### Step 1: Create a New Web API Project

Create a new Web API project and add the reference to  `GenericApiClient`  as explained earlier in the installation section.

### Step 2: Create the ProductController

```csharp
using Microsoft.AspNetCore.Mvc;
using ApiBuilderExample.Models;

namespace ApiBuilderExample.Controllers;

[ApiController]
[Route("api/[controller]")]
public class ProductController : ControllerBase
{
    private readonly ProductService _productService;

    public ProductController(ProductService productService)
    {
        _productService = productService;
    }

    // GET: api/product/all
    [HttpGet("all")]
    public async Task<IActionResult> GetAllProducts()
    {
        var products = await _productService.GetAllProductsAsync();
        return Ok(products);
    }

    // POST: api/product
    [HttpPost]
    public async Task<IActionResult> AddProduct([FromBody] Product newProduct)
    {
        var addedProductId = await _productService.AddProductAsync(newProduct);
        return Ok(new { ProductId = addedProductId });
    }


    [HttpDelete("deleteProduct")]
    public async Task<IActionResult> DeleteProductAsync(string productId)
    {
        var result = await _productService.DeleteProductAsync(productId);
        return Ok(result);
    }
}
```

### Step 3: Create the ProductService Layer

```csharp
using GenericApiClient;
using Microsoft.Extensions.Options;
using ApiBuilderExample.Models;

namespace ApiBuilderExample;

public class ProductService
{
    private readonly IOptions<AppConfigurations> _config;
    private readonly ILogger<ProductService> _logger;

    public ProductService(IOptions<AppConfigurations> config, ILogger<ProductService> logger)
    {
        _config = config;
        _logger = logger;
    }

    public async Task<List<Product>> GetAllProductsAsync()
    {
        var response = await new ApiCallBuilder<List<Product>, ProductsErrorResponse>()
            .SetEndpoint($"{_config.Value.ProductServiceConfigurations.URI}/api/Product")
            .SetMethod(HttpMethod.Get)
            .SetHeaders(GetDefaultHeaders())
            .SetContentType(MediaType.ApplicationJson)
            .AddStatusHandler(System.Net.HttpStatusCode.OK, async res =>
            {
                _logger.LogInformation("[ProductService] Successfully retrieved products");
                return res.SuccessResponse ?? new List<Product>();
            })
            .AddStatusHandler(System.Net.HttpStatusCode.BadRequest, async res =>
            {
                _logger.LogWarning("[ProductService] Bad request while retrieving products");
                return new List<Product>();
            })
            .SetDefaultHandler(async (statusCode, errorMessage) =>
            {
                _logger.LogError("[ProductService] Failed to retrieve products. Status code: {StatusCode}, Error: {Error}", statusCode, errorMessage);
                return new List<Product>();
            })
            .ExecuteAsync<List<Product>>();

        return (List<Product>)response;
    }

    public async Task<int> AddProductAsync(Product product)
    {
        var response = await new ApiCallBuilder<ProductResponse, ProductsErrorResponse>()
            .SetEndpoint($"{_config.Value.ProductServiceConfigurations.URI}/api/product")
            .SetMethod(HttpMethod.Post)
            .SetHeaders(GetDefaultHeaders())
            .SetBody(product)
            .SetContentType(MediaType.ApplicationJson)
            .AddStatusHandler(System.Net.HttpStatusCode.Created, async res =>
            {
                int id = 0;
                if (res.SuccessResponse?.Data != null)
                    id = (int) res.SuccessResponse?.Data.Id;
                _logger.LogInformation("[ProductService] Successfully added a new product");
                return id;
            })
            .AddStatusHandler(System.Net.HttpStatusCode.BadRequest, async res =>
            {
                _logger.LogWarning("[ProductService] Bad request when adding a product");
                return -1;
            })
            .SetDefaultHandler(async (statusCode, errorMessage) =>
            {
                _logger.LogError("[ProductService] Failed to add product. Status code: {StatusCode}, Error: {Error}", statusCode, errorMessage);
                return -1;
            })
            .ExecuteAsync<int>();

        return (int)response;
    }

    public async Task<DeleteProductResponse> DeleteProductAsync(string productId)
    {
        
        var result = await new ApiCallBuilder<DeleteProductResponse, ProductsErrorResponse>()
            .SetEndpoint($"{_config.Value.ProductServiceConfigurations.URI}/api/product/{productId}")
            .SetMethod(HttpMethod.Delete)
            .SetHeaders(GetDefaultHeaders())
            .AddStatusHandler(System.Net.HttpStatusCode.OK, async response =>
            {
                _logger.LogInformation("[ProductService] Successfully deleted the product");
                return response.SuccessResponse;
            })
            .AddStatusHandler(System.Net.HttpStatusCode.BadRequest, async response =>
            {
                _logger.LogWarning("[ProductService] Bad request when deleting product error {@error}",
                    response.RawContent);
                return new DeleteProductResponse() {Message = "[ProductService] Bad request when deleting product"};
            })
            .SetDefaultHandler<DeleteProductResponse>(async (statusCode, errorMessage) =>
            {
                _logger.LogWarning(
                    "[ProductService] Failed to delete product. Unhandled status code: {@statusCode}, error: {@error}",
                    statusCode, errorMessage);
                return new DeleteProductResponse() { Message = "Failed to delete product" };
            })
            .ExecuteAsync<DeleteProductResponse>();

        return (DeleteProductResponse)result;
    }

    private Dictionary<string, string> GetDefaultHeaders()
    {
        return new Dictionary<string, string>
        {
            { "Authorization", $"Bearer {_config.Value.ProductServiceConfigurations.BaseToken}" }
        };
    }
}
```

###  Step 4: Create the Models 

Note that for `ProductResponse`, this is just an example and you need to put the fields based on the response coming from the API. 

```csharp
using System.Text.Json.Serialization;

namespace ApiBuilderExample.Models;

public class Product
{
    [property: JsonPropertyName("id")]
    public int Id { get; set; }
    [property: JsonPropertyName("name")]
    public string Name { get; set; }
    [property: JsonPropertyName("price")]
    public decimal Price { get; set; }
    [property: JsonPropertyName("category")]
    public string Category { get; set; }
    [property: JsonPropertyName("inStock")]
    public bool InStock { get; set; }
}

public class ProductsErrorResponse
{
    public string Message { get; set; }
    public string ErrorCode { get; set; }
}


public class ProductResponse
{
    [property: JsonPropertyName("message")]
    public string Message { get; set; }
    [property: JsonPropertyName("data")]
    public Product Data { get; set; }
}


public record DeleteProductResponse
{
    [JsonPropertyName("message")]
    public string Message { get; set; }
};
```


###  Step 5: Create the AppConfigurations File

```csharp
namespace MyApp.Config
{
    public class AppConfigurations
    {
        public ProductServiceConfigurations ProductServiceConfigurations { get; set; }
        public LoggingConfigurations LoggingConfigurations { get; set; }
    }

    public class ProductServiceConfigurations
    {
        public string BaseToken { get; set; }
        public string URI { get; set; }
    }

    public class LoggingConfigurations
    {
        public string LogLevel { get; set; }
        public string ConnectionString { get; set; }
    }
}
```

### Step 6: Add Configuration to  `appsettings.json`

Update the  `appsettings.json`  file with your configuration values:

```csharp
{
  "AppConfigurations": {
    "ProductServiceConfigurations": {
      "BaseToken": "your-token-here",
      "URI": "https://api.yourproductservice.com"
    },
    "LoggingConfigurations": {
      "LogLevel": "Information",
      "ConnectionString": "your-logging-connection-string"
    }
  }
}
```

### Step 7: Add the necessary services in  `program.cs`

```csharp
builder.Services.Configure<AppConfigurations>(builder.Configuration.GetSection("AppConfigurations"));
var appConfig = builder.Configuration.GetSection("AppConfigurations").Get<AppConfigurations>();
builder.Services.AddSingleton(appConfig);
builder.Services.AddSingleton<ProductService>();
```


## Features

- Support for various content types (JSON, Form URL Encoded, etc.)
- Automatic serialization and deserialization of request/response bodies
- Flexible response handling with typed success and error responses
- Query parameter support
- Custom header support
