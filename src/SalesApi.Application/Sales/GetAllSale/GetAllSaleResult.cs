using SalesApi.Domain.Entities;

namespace SalesApi.Application.Sales.GetAllSale;

/// <summary>
/// Response model for the GetAllSale operation.
/// </summary>
public class GetAllSaleResult
{
    /// <summary>
    /// The sales in the requested page, without their items.
    /// </summary>
    public List<Sale> Data { get; set; } = new List<Sale>();

    /// <summary>
    /// Total number of sales matching the filters, across all pages.
    /// </summary>
    public int TotalCount { get; set; }
}