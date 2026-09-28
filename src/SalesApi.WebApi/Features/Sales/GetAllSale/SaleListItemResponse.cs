namespace SalesApi.WebApi.Features.Sales.GetAllSale;

/// <summary>
/// Flat, API-facing shape for one sale in a list response - avoids exposing
/// the Sale aggregate (private setters, owned value objects) directly as JSON.
/// </summary>
public class SaleListItemResponse
{
    public Guid Id { get; set; }
    public string SaleNumber { get; set; } = string.Empty;
    public DateTimeOffset Date { get; set; }
    public string CustomerName { get; set; } = string.Empty;
    public string BranchName { get; set; } = string.Empty;
    public decimal TotalAmount { get; set; }
    public string Status { get; set; } = string.Empty;
}
