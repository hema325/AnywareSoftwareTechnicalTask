using Application.Users.Dtos;
using MediatR;

namespace Application.Users.Queries.GetUserById
{
    public record GetUserByIdQuery(int Id) : IRequest<UserDto>;
}
