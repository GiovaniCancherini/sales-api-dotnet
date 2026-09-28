using MediatR;

namespace SalesApi.Application.Sales.GetAllSale;

/// <summary>
/// Query for retrieving sales, with optional filtering and ordering.
/// Pagination (_page/_size) is applied at the WebApi layer, same as ListUsers.
/// </summary>
public class GetAllSaleQuery : IRequest<GetAllSaleResult>
{
    /// <summary>
    /// Ordering clause, e.g. "totalAmount desc, saleNumber" (comma-separated,
    /// each field optionally followed by asc/desc, default asc).
    /// </summary>
    public string? Order { get; set; }

    /// <summary>
    /// Filters by customer name. Supports wildcard prefix/suffix: "Jo*", "*Silva", "*Silva*".
    /// </summary>
    public string? CustomerName { get; set; }

    /// <summary>Filters by exact sale status (Active/Cancelled).</summary>
    public string? Status { get; set; }

    public decimal? MinTotalAmount { get; set; }
    public decimal? MaxTotalAmount { get; set; }
    public DateTime? MinDate { get; set; }
    public DateTime? MaxDate { get; set; }
}
