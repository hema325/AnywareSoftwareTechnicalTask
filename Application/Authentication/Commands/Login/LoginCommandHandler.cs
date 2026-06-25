using Application.Common.Authentication;
using Application.Common.Contracts;
using Application.Common.Exceptions;
using MediatR;
using Microsoft.EntityFrameworkCore;

namespace Application.Authentication.Commands.Login
{
    internal sealed class LoginCommandHandler : IRequestHandler<LoginCommand, TokenResult>
    {
        private readonly IAppDbContext _context;
        private readonly IPasswordHasher _passwordHasher;
        private readonly ITokenGenerator _tokenGenerator;

        public LoginCommandHandler(
            IAppDbContext context,
            IPasswordHasher passwordHasher,
            ITokenGenerator tokenGenerator)
        {
            _context = context;
            _passwordHasher = passwordHasher;
            _tokenGenerator = tokenGenerator;
        }

        public async Task<TokenResult> Handle(LoginCommand request, CancellationToken cancellationToken)
        {
            var user = await _context.Users
                .FirstOrDefaultAsync(u => u.Email == request.Email, cancellationToken);

            if (user is null || !_passwordHasher.VerifyHashedPassword(user.HashedPassword, request.Password))
            {
                throw new UnauthorizedException("Invalid email or password.");
            }

            var token = _tokenGenerator.Generate(user);

            _context.RefreshTokens.Add(new Domain.Entities.RefreshToken
            {
                Token = token.RefreshToken,
                ExpiresAt = token.RefreshTokenExpiresAt,
                UserId = user.Id
            });

            await _context.SaveChangesAsync(cancellationToken);

            return token;
        }
    }
}
