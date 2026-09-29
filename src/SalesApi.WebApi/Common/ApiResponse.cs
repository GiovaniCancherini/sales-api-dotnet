namespace SalesApi.WebApi.Common;

/// <summary>
/// Success response envelope. Error responses use ApiErrorResponse instead -
/// Errors used to live here for the failure case, but every failure path now
/// returns ApiErrorResponse, so it had no remaining caller.
/// </summary>
public class ApiResponse
{
    public bool Success { get; set; }
    public string Message { get; set; } = string.Empty;
}
