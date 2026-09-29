using MediatR;

namespace SalesApi.Application.Users.GetAllUser;

/// <summary>
/// Query for retrieving users, with optional ordering.
/// Pagination (_page/_size) is applied at the WebApi layer, same as GetAllSale.
/// </summary>
public record GetAllUserQuery : IRequest<List<GetAllUserResult>>
{
    /// <summary>
    /// Ordering clause, e.g. "name desc, email" (comma-separated, each field
    /// optionally followed by asc/desc, default asc). Matches GetAllSaleQuery's
    /// _order convention.
    /// </summary>
    public string? Order { get; set; }
}
