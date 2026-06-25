using Application.Common.Authentication;
using MediatR;

namespace Application.Authentication.Commands.RefreshToken
{
    public record RefreshTokenCommand(string RefreshToken) : IRequest<TokenResult>;
}
