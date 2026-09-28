namespace SalesApi.WebApi.Features.Sales.CreateSale;

/// <summary>
/// Represents one item of a sale being created.
/// </summary>
public class CreateSaleItemRequest
{
    /// <summary>
    /// Gets or sets the ID of the product.
    /// </summary>
    public string ProductId { get; set; } = string.Empty;

    /// <summary>
    /// Gets or sets the name of the product.
    /// </summary>
    public string ProductName { get; set; } = string.Empty;

    /// <summary>
    /// Gets or sets the SKU of the product.
    /// </summary>
    public string ProductSku { get; set; } = string.Empty;

    /// <summary>
    /// Gets or sets the quantity of the product.
    /// </summary>
    public int Quantity { get; set; }

    /// <summary>
    /// Gets or sets the unit price of the product.
    /// </summary>
    public decimal UnitPrice { get; set; }
}
