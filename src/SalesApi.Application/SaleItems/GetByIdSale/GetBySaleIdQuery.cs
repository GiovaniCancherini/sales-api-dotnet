using MediatR;

namespace SalesApi.Application.SaleItems.GetByIdSale;

/// <summary>
/// Command for retrieving a sale item by their ID
/// </summary>
public record GetByIdSaleQuery : IRequest<GetByIdSaleResult>
{
    /// <summary>
    /// The unique identifier of the sale item to retrieve.
    /// </summary>
    public Guid Id { get; init; }

    /// <summary>
    /// The unique identifier of the parent sale.
    /// </summary>
    public Guid SaleId { get; init; }

    /// <summary>
    /// Initializes a new instance of GetByIdSale.
    /// </summary>
    /// <param name="id">The unique ID of the sale item.</param>
    /// <param name="saleId">The ID of the parent sale.</param>
    public GetByIdSaleQuery(Guid id, Guid saleId)
    {
        Id = id;
        SaleId = saleId;
    }
}
