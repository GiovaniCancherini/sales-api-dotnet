using MediatR;

namespace SalesApi.Application.Users.GetAllUser;

/// <summary>
/// Command for retrieving a user by their ID
/// </summary>
public record GetAllUserQuery : IRequest<List<GetAllUserResult>>
{
    public int PageNumber { get; init; } = 1;
    public int PageSize { get; init; } = 10;

    /// <summary>
    /// Initializes a new instance of GetAllUserCommand
    /// </summary>
    public GetAllUserQuery(int pageNumber = 1, int pageSize = 10)
    {
        PageNumber = pageNumber;
        PageSize = pageSize;
    }
}
