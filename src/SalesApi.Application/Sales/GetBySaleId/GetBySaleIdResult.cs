namespace SalesApi.Application.Sales.GetBySaleId;

/// <summary>
/// Response model for GetBySaleId operation - full sale detail, including items.
/// Was just { Id } before, missing everything the README's Use Case actually
/// requires the API to inform (SaleNumber, Date, Customer, Branch, items, etc.).
/// </summary>
public class GetBySaleIdResult
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
    public List<GetBySaleIdItemResult> Items { get; set; } = new();
}
