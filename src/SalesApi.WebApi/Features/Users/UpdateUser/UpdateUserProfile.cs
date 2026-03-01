using SalesApi.Application.Users.UpdateUser;
using AutoMapper;

namespace SalesApi.WebApi.Features.Users.UpdateUser;

/// <summary>
/// Profile for mapping between Application and API UpdateUser responses
/// </summary>
public class UpdateUserProfile : Profile
{
    /// <summary>
    /// Initializes the mappings for UpdateUser feature
    /// </summary>
    public UpdateUserProfile()
    {
        CreateMap<UpdateUserRequest, UpdateUserCommand>();
    }
}
