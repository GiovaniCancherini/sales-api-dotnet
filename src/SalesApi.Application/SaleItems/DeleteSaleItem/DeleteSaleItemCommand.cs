using MediatR;

namespace SalesApi.Application.SaleItems.DeleteSaleItem;

/// <summary>
/// Command for deleting a sale item
/// </summary>
public record DeleteSaleItemCommand : IRequest<DeleteSaleItemResponse>
{
    /// <summary>
    /// The unique identifier of the sale item to delete
    /// </summary>
    public Guid Id { get; }

    /// <summary>
    /// Initializes a new instance of DeleteSaleItemCommand
    /// </summary>
    /// <param name="id">The ID of the sale item to delete</param>
    public DeleteSaleItemCommand(Guid id)
    {
        Id = id;
    }
}
