using MediatR;

namespace SalesApi.Application.Users.GetAllUser;

/// <summary>
/// Command for retrieving a user by their ID
/// </summary>
public record GetAllUserQuery : IRequest<List<GetAllUserResult>>
{

    /// <summary>
    /// Initializes a new instance of GetAllUserCommand
    /// </summary>
    public GetAllUserQuery()
    { }
}
