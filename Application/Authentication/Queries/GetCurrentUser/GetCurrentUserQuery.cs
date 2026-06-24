using Application.Authentication.Dtos;
using MediatR;

namespace Application.Authentication.Queries.GetCurrentUser
{
    public record GetCurrentUserQuery : IRequest<UserDto>;
}
