using AutoMapper;

namespace SalesApi.WebApi.Features.Users.ListUsers;

/// <summary>
/// Profile for mapping ListUsers feature requests to commands
/// </summary>
public class ListUsersProfile : Profile
{
    /// <summary>
    /// Initializes the mappings for ListUsers feature
    /// </summary>
    public ListUsersProfile()
    {
        CreateMap<ListUsersRequest, Application.Users.GetAllUser.GetAllUserQuery>();
    }
}
