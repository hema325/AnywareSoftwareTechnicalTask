using Application.Users.Dtos;
using MediatR;

namespace Application.Users.Queries.GetUsers
{
    public record GetUsersQuery : IRequest<IReadOnlyList<UserDto>>;
}
