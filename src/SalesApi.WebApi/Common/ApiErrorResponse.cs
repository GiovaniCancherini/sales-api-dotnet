namespace SalesApi.WebApi.Common;

/// <summary>
/// Error response shape for every non-2xx response, per the API's
/// "Error Response Format": a machine-readable type, a short human-readable
/// summary, and a detail specific to this occurrence. Success responses are
/// unaffected - the error format does not define a success envelope, so
/// ApiResponse/ApiResponseWithData (already tested end-to-end) stay as-is.
/// </summary>
public class ApiErrorResponse
{
    public string Type { get; set; } = string.Empty;
    public string Error { get; set; } = string.Empty;
    public string Detail { get; set; } = string.Empty;
}
