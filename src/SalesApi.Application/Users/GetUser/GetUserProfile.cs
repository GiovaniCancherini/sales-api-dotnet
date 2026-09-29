using AutoMapper;
using SalesApi.Domain.Entities;

namespace SalesApi.Application.Users.GetUser;

/// <summary>
/// Profile for mapping between User entity and GetUserResponse
/// </summary>
public class GetUserProfile : Profile
{
    /// <summary>
    /// Initializes the mappings for GetUser operation
    /// </summary>
    public GetUserProfile()
    {
        // User.Username doesn't match GetUserResult.Name by convention -
        // without this, Name comes back empty.
        CreateMap<User, GetUserResult>()
            .ForMember(dest => dest.Name, opt => opt.MapFrom(src => src.Username));
    }
}
