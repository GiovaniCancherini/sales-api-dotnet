using SalesApi.Domain.Entities;
using SalesApi.Domain.Enums;

namespace SalesApi.Application.SaleItems.GetByIdSale;

/// <summary>
/// Response model for GetBySaleItemId operation
/// </summary>
public class GetByIdSaleResult
{
    /// <summary>
    /// Gets or sets the list of sales retrieved from the operation.
    /// </summary>
    public List<SaleItem> Data { get; set; } = new List<SaleItem>();

}
