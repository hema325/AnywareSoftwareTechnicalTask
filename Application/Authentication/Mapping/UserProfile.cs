using Application.Authentication.Dtos;
using AutoMapper;

namespace Application.Authentication.Mapping
{
    public sealed class UserProfile : Profile
    {
        public UserProfile()
        {
            CreateMap<User, UserDto>()
                .ForMember(dto => dto.Role, options => options.MapFrom(user => user.Role.ToString()));
        }
    }
}
