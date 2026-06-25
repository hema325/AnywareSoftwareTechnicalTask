using Application.Common.Authentication;
using Application.Common.Contracts;
using MediatR;

namespace Application.Authentication.Commands.Register
{
    internal sealed class RegisterCommandHandler : IRequestHandler<RegisterCommand, TokenResult>
    {
        private readonly IAppDbContext _context;
        private readonly IPasswordHasher _passwordHasher;
        private readonly ITokenGenerator _tokenGenerator;

        public RegisterCommandHandler(
            IAppDbContext context,
            IPasswordHasher passwordHasher,
            ITokenGenerator tokenGenerator)
        {
            _context = context;
            _passwordHasher = passwordHasher;
            _tokenGenerator = tokenGenerator;
        }

        public async Task<TokenResult> Handle(RegisterCommand request, CancellationToken cancellationToken)
        {
            var user = new User
            {
                Name = request.Name,
                Email = request.Email,
                HashedPassword = _passwordHasher.HashPassword(request.Password),
                Role = UserRole.User
            };

            _context.Users.Add(user);
            await _context.SaveChangesAsync(cancellationToken);

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
