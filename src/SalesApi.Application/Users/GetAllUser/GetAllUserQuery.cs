using MediatR;

namespace SalesApi.Application.Users.GetAllUser;

/// <summary>
/// Query for retrieving one page of users, with optional ordering.
/// Returns the page plus the total user count.
/// </summary>
public record GetAllUserQuery : IRequest<(List<GetAllUserResult> Items, int TotalCount)>
{
    public int Page { get; set; } = 1;
    public int Size { get; set; } = 10;

    /// <summary>
    /// Ordering clause, e.g. "name desc, email" (comma-separated, each field
    /// optionally followed by asc/desc, default asc). Matches GetAllSaleQuery's
    /// _order convention.
    /// </summary>
    public string? Order { get; set; }
}
