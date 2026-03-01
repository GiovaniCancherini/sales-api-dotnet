using AutoMapper;
using SalesApi.Domain.Entities;
using SalesApi.WebApi.Features.Users.ListUsers;

namespace SalesApi.WebApi.Mappings
{
    public class ListUsersProfile : Profile
    {
        public ListUsersProfile()
        {
            CreateMap<User, ListUsersResponse>();
        }
    }
}
