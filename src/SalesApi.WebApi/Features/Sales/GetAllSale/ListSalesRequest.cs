using Microsoft.AspNetCore.Mvc;

namespace SalesApi.WebApi.Features.Sales.GetAllSale;

/// <summary>
/// Query parameters for listing sales, following the API's convention:
/// _page/_size for pagination, _order for sorting, _min/_max prefixes for
/// numeric/date range filters, plain field=value for exact filters.
/// </summary>
public class ListSalesRequest
{
    [FromQuery(Name = "_page")]
    public int Page { get; set; } = 1;

    [FromQuery(Name = "_size")]
    public int Size { get; set; } = 10;

    [FromQuery(Name = "_order")]
    public string? Order { get; set; }

    [FromQuery(Name = "customerName")]
    public string? CustomerName { get; set; }

    [FromQuery(Name = "status")]
    public string? Status { get; set; }

    [FromQuery(Name = "_minTotalAmount")]
    public decimal? MinTotalAmount { get; set; }

    [FromQuery(Name = "_maxTotalAmount")]
    public decimal? MaxTotalAmount { get; set; }

    [FromQuery(Name = "_minDate")]
    public DateTime? MinDate { get; set; }

    [FromQuery(Name = "_maxDate")]
    public DateTime? MaxDate { get; set; }
}
