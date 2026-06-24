using Application.Common.Contracts;
using Application.Common.Exceptions;
using Application.TaskItems.Dtos;
using AutoMapper;
using MediatR;
using Microsoft.EntityFrameworkCore;

namespace Application.TaskItems.Queries.GetTaskById
{
    internal sealed class GetTaskByIdQueryHandler : IRequestHandler<GetTaskByIdQuery, TaskItemDto>
    {
        private readonly IAppDbContext _context;
        private readonly ICurrentUser _currentUser;
        private readonly IMapper _mapper;

        public GetTaskByIdQueryHandler(IAppDbContext context, ICurrentUser currentUser, IMapper mapper)
        {
            _context = context;
            _currentUser = currentUser;
            _mapper = mapper;
        }

        public async Task<TaskItemDto> Handle(GetTaskByIdQuery request, CancellationToken cancellationToken)
        {
            var task = await _context.TaskItems.FirstOrDefaultAsync(
                t => t.Id == request.Id && t.OwnerId == _currentUser.Id,
                cancellationToken);

            if (task is null)
            {
                throw new NotFoundException($"Task {request.Id} was not found.");
            }

            return _mapper.Map<TaskItemDto>(task);
        }
    }
}
