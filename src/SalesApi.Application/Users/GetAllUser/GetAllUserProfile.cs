using AutoMapper;
using SalesApi.Domain.Entities;

namespace SalesApi.Application.Users.GetAllUser;

/// <summary>
/// Profile for mapping between User entity and GetAllUserResponse
/// </summary>
public class GetAllUserProfile : Profile
{
    /// <summary>
    /// Initializes the mappings for GetAllUser operation
    /// </summary>
    public GetAllUserProfile()
    {
        // Item-to-item map: AutoMapper handles List<User> -> List<GetAllUserResult>
        // on its own from this, no separate List<T> mapping needed.
        CreateMap<User, GetAllUserResult>()
            .ForMember(dest => dest.Name, opt => opt.MapFrom(src => src.Username));
    }
}
