using Application.Common.Contracts;
using Application.Common.Exceptions;
using Application.Authentication.Dtos;
using MediatR;
using Microsoft.EntityFrameworkCore;

namespace Application.Authentication.Queries.GetCurrentUser
{
    internal sealed class GetCurrentUserQueryHandler : IRequestHandler<GetCurrentUserQuery, UserDto>
    {
        private readonly IAppDbContext _context;
        private readonly ICurrentUser _currentUser;

        public GetCurrentUserQueryHandler(IAppDbContext context, ICurrentUser currentUser)
        {
            _context = context;
            _currentUser = currentUser;
        }

        public async Task<UserDto> Handle(GetCurrentUserQuery request, CancellationToken cancellationToken)
        {
            var user = await _context.Users
                .FirstOrDefaultAsync(u => u.Id == _currentUser.Id, cancellationToken);

            if (user is null)
            {
                throw new NotFoundException("User not found.");
            }

            return new UserDto
            {
                Id = user.Id,
                Name = user.Name,
                Email = user.Email,
                Role = user.Role.ToString(),
                CreatedAt = user.CreatedAt
            };
        }
    }
}
