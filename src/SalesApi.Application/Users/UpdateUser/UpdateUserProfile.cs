using AutoMapper;
using SalesApi.Domain.Entities;

namespace SalesApi.Application.Users.UpdateUser;

/// <summary>
/// Profile for mapping UpdateUserCommand onto an existing User entity.
/// </summary>
public class UpdateUserProfile : Profile
{
    public UpdateUserProfile()
    {
        // Mapped onto an already-loaded, tracked User (see UpdateUserHandler),
        // never used to construct a new instance - Id/CreatedAt/UpdatedAt/Events
        // must survive the update untouched by command data.
        CreateMap<UpdateUserCommand, User>()
            .ForMember(dest => dest.Id, opt => opt.Ignore())
            .ForMember(dest => dest.CreatedAt, opt => opt.Ignore())
            .ForMember(dest => dest.UpdatedAt, opt => opt.Ignore())
            .ForMember(dest => dest.Events, opt => opt.Ignore());
    }
}
