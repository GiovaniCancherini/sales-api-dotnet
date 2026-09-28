namespace SalesApi.WebApi.Features.Sales.GetBySaleId;

/// <summary>
/// Full detail of one sale, including its items.
/// </summary>
public class SaleDetailResponse
{
    public Guid Id { get; set; }
    public string SaleNumber { get; set; } = string.Empty;
    public DateTimeOffset Date { get; set; }
    public Guid CustomerId { get; set; }
    public string CustomerName { get; set; } = string.Empty;
    public Guid BranchId { get; set; }
    public string BranchName { get; set; } = string.Empty;
    public decimal TotalAmount { get; set; }
    public string Status { get; set; } = string.Empty;
    public List<SaleDetailItemResponse> Items { get; set; } = new();
}
