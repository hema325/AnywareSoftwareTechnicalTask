using Application.Common.Authentication;
using MediatR;

namespace Application.Authentication.Commands.Register
{
    public record RegisterCommand(string Name, string Email, string Password) : IRequest<JwtToken>;
}
