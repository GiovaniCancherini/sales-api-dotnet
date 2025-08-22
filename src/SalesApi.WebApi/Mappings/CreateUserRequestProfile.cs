using SalesApi.Application.Users.CreateUser;
using SalesApi.WebApi.Features.Users.CreateUser;
using AutoMapper;

namespace SalesApi.WebApi.Mappings;

public class CreateUserRequestProfile : Profile
{
    public CreateUserRequestProfile()
    {
        CreateMap<CreateUserRequest, CreateUserCommand>();
    }
}