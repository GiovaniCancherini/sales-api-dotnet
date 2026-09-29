using AutoMapper;
using SalesApi.Domain.Entities;

namespace SalesApi.Application.Users.CreateUser;

/// <summary>
/// Profile for mapping between User entity and CreateUserResponse
/// </summary>
public class CreateUserProfile : Profile
{
    /// <summary>
    /// Initializes the mappings for CreateUser operation
    /// </summary>
    public CreateUserProfile()
    {
        // Id is DB-generated (gen_random_uuid()), CreatedAt/UpdatedAt/Events
        // are set by User's own constructor/domain methods - none of them
        // should ever come from client input.
        CreateMap<CreateUserCommand, User>()
            .ForMember(dest => dest.Id, opt => opt.Ignore())
            .ForMember(dest => dest.CreatedAt, opt => opt.Ignore())
            .ForMember(dest => dest.UpdatedAt, opt => opt.Ignore())
            .ForMember(dest => dest.Events, opt => opt.Ignore());

        // User.Username doesn't match CreateUserResult.Name by convention.
        CreateMap<User, CreateUserResult>()
            .ForMember(dest => dest.Name, opt => opt.MapFrom(src => src.Username));
    }
}
