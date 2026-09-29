using MediatR;

namespace SalesApi.Application.Sales.GetAllSale;

/// <summary>
/// Query for retrieving one page of sales, with optional filtering and ordering.
/// </summary>
public class GetAllSaleQuery : IRequest<GetAllSaleResult>
{
    public int Page { get; set; } = 1;
    public int Size { get; set; } = 10;

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
