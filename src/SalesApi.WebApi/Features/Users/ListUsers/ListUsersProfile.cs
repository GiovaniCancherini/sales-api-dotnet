using SalesApi.Application.Users.GetAllUser;
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
        CreateMap<ListUsersRequest, GetAllUserQuery>();

        // Was missing entirely - ListUsers always threw AutoMapperMappingException.
        CreateMap<GetAllUserResult, ListUsersResponse>();
    }
}
