using Application.Common.Authentication;
using Application.Common.Contracts;
using Application.Common.Exceptions;
using MediatR;
using Microsoft.EntityFrameworkCore;

namespace Application.Authentication.Commands.RefreshToken
{
    internal sealed class RefreshTokenCommandHandler : IRequestHandler<RefreshTokenCommand, TokenResult>
    {
        private readonly IAppDbContext _context;
        private readonly ITokenGenerator _tokenGenerator;

        public RefreshTokenCommandHandler(IAppDbContext context, ITokenGenerator tokenGenerator)
        {
            _context = context;
            _tokenGenerator = tokenGenerator;
        }

        public async Task<TokenResult> Handle(RefreshTokenCommand request, CancellationToken cancellationToken)
        {
            var storedToken = await _context.RefreshTokens
                .Include(token => token.User)
                .FirstOrDefaultAsync(token => token.Token == request.RefreshToken, cancellationToken);

            if (storedToken is null || !storedToken.IsActive)
            {
                throw new UnauthorizedException("Invalid refresh token.");
            }

            var newToken = _tokenGenerator.Generate(storedToken.User);
            
            storedToken.RevokedAt = DateTime.UtcNow;
            
            _context.RefreshTokens.Add(new Domain.Entities.RefreshToken
            {
                Token = newToken.RefreshToken,
                ExpiresAt = newToken.RefreshTokenExpiresAt,
                UserId = storedToken.UserId
            });

            await _context.SaveChangesAsync(cancellationToken);

            return newToken;
        }
    }
}
