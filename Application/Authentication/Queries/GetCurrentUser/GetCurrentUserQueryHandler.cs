using Application.Common.Contracts;
using Application.Common.Exceptions;
using Application.Authentication.Dtos;
using AutoMapper;
using MediatR;
using Microsoft.EntityFrameworkCore;

namespace Application.Authentication.Queries.GetCurrentUser
{
    internal sealed class GetCurrentUserQueryHandler : IRequestHandler<GetCurrentUserQuery, UserDto>
    {
        private readonly IAppDbContext _context;
        private readonly ICurrentUser _currentUser;
        private readonly IMapper _mapper;

        public GetCurrentUserQueryHandler(IAppDbContext context, ICurrentUser currentUser, IMapper mapper)
        {
            _context = context;
            _currentUser = currentUser;
            _mapper = mapper;
        }

        public async Task<UserDto> Handle(GetCurrentUserQuery request, CancellationToken cancellationToken)
        {
            var user = await _context.Users
                .FirstOrDefaultAsync(u => u.Id == _currentUser.Id, cancellationToken);

            if (user is null)
            {
                throw new NotFoundException("User not found.");
            }

            return _mapper.Map<UserDto>(user);
        }
    }
}
