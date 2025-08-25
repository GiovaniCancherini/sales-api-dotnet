using MediatR;

namespace SalesApi.Application.Sales.GetAllSale;

/// <summary>
/// Query for retrieving all sales.
/// </summary>
/// <remarks>
/// This query does not require any parameters as it is designed to
/// return a complete list of all available sales in the system.
/// </remarks>
public record GetAllSaleQuery : IRequest<GetAllSaleResult>
{
    // no params, empty.
}
