using SalesApi.Domain.Entities;

namespace SalesApi.Application.SaleItems.GetAllSaleItem;

/// <summary>
/// Response model for the GetAllSaleItem operation.
/// </summary>
public class GetAllSaleItemResult
{
    /// <summary>
    /// Gets or sets the list of sales retrieved from the operation.
    /// </summary>
    public List<SaleItem> Data { get; set; } = new List<SaleItem>();
}