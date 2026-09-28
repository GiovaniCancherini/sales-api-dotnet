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
        // Was CreateMap<User, List<GetAllUserResult>>() - mapping a single User
        // to a List<T> is meaningless; AutoMapper needs the item-to-item map
        // (User -> GetAllUserResult) and handles List<User> -> List<GetAllUserResult>
        // on its own. Same Username/Name mismatch as GetUserProfile.
        CreateMap<User, GetAllUserResult>()
            .ForMember(dest => dest.Name, opt => opt.MapFrom(src => src.Username));
    }
}
