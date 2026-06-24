using MediatR;

namespace Application.Users.Commands.CreateUser
{
    public record CreateUserCommand(string Name, string Email, string Password, UserRole Role) : IRequest<int>;
}
