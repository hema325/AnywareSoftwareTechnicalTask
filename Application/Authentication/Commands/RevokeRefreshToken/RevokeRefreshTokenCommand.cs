using MediatR;

namespace Application.Authentication.Commands.RevokeRefreshToken
{
    public record RevokeRefreshTokenCommand(string RefreshToken) : IRequest;
}
