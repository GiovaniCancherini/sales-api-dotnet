using MediatR;

namespace SalesApi.Application.SaleItems.GetBySaleItemId;

/// <summary>
/// Command for retrieving a sale by their ID
/// </summary>
public record GetBySaleItemIdQuery : IRequest<GetBySaleItemIdResult>
{
    /// <summary>
    /// The unique identifier of the sale to retrieve
    /// </summary>
    public Guid Id { get; }

    /// <summary>
    /// Initializes a new instance of GetBySaleItemIdCommand
    /// </summary>
    /// <param name="id">The ID of the sale to retrieve</param>
    public GetBySaleItemIdQuery(Guid id)
    {
        Id = id;
    }
}
