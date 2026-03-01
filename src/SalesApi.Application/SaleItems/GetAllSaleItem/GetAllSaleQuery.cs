using MediatR;

namespace SalesApi.Application.SaleItems.GetAllSaleItem;

/// <summary>
/// Query for retrieving all sales items.
/// </summary>
/// <remarks>
/// This query does not require any parameters as it is designed to
/// return a complete list of all available sales in the system.
/// </remarks>
public record GetAllSaleItemQuery : IRequest<GetAllSaleItemResult>
{
    // no params, empty.
}
