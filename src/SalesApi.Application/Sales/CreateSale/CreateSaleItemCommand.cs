namespace SalesApi.Application.Sales.CreateSale;

/// <summary>
/// Flat, client-facing shape for one item of a sale being created.
/// The Handler builds the real domain SaleItem (via ExternalProduct) from this.
/// No Discount field: it's a business rule computed from Quantity by
/// SaleItem itself, not something the client gets to set.
/// </summary>
public class CreateSaleItemCommand
{
    public string ProductId { get; set; } = string.Empty;
    public string ProductName { get; set; } = string.Empty;
    public string ProductSku { get; set; } = string.Empty;
    public int Quantity { get; set; }
    public decimal UnitPrice { get; set; }
}
