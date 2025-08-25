using MediatR;

namespace SalesApi.Application.Sales.GetBySaleId;

/// <summary>
/// Command for retrieving a sale by their ID
/// </summary>
public record GetBySaleIdCommand : IRequest<GetBySaleIdResult>
{
    /// <summary>
    /// The unique identifier of the sale to retrieve
    /// </summary>
    public Guid Id { get; }

    /// <summary>
    /// Initializes a new instance of GetBySaleIdCommand
    /// </summary>
    /// <param name="id">The ID of the sale to retrieve</param>
    public GetBySaleIdCommand(Guid id)
    {
        Id = id;
    }
}
