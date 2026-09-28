using MediatR;

namespace SalesApi.Application.Sales.CancelSale;

/// <summary>
/// Command for cancelling a sale
/// </summary>
public record CancelSaleCommand : IRequest<CancelSaleResponse>
{
    /// <summary>
    /// The unique identifier of the sale to cancel
    /// </summary>
    public Guid Id { get; }

    /// <summary>
    /// Initializes a new instance of CancelSaleCommand
    /// </summary>
    /// <param name="id">The ID of the sale to cancel</param>
    public CancelSaleCommand(Guid id)
    {
        Id = id;
    }
}
