using System.Net;

namespace GenericApiClient;

public class ApiResponse<TSuccessResponse, TFailureResponse>
{
    public HttpStatusCode StatusCode { get; set; }
    public bool IsSuccessful => (int)StatusCode >= 200 && (int)StatusCode < 300;
    public TSuccessResponse SuccessResponse { get; set; }
    public TFailureResponse FailureResponse { get; set; }
    public string RawContent { get; set; }
    public bool HasContent => !string.IsNullOrEmpty(RawContent);
    public bool IsAmbiguous { get; set; }
    public string ErrorMessage { get; set; }
    public Exception Exception { get; set; }
}