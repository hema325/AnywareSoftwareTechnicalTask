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
        private readonly ICache _cache;

        public GetTaskByIdQueryHandler(IAppDbContext context, ICurrentUser currentUser, IMapper mapper, ICache cache)
        {
            _context = context;
            _currentUser = currentUser;
            _mapper = mapper;
            _cache = cache;
        }

        public async Task<TaskItemDto> Handle(GetTaskByIdQuery request, CancellationToken cancellationToken)
        {
            var key = $"TaskItem_{request.Id}";
            var task = await _cache.GetAsync<TaskItem>(key, cancellationToken);

            if(task == null)
            {
                task = await _context.TaskItems.FirstOrDefaultAsync(
                    t => t.Id == request.Id && t.OwnerId == _currentUser.Id,
                    cancellationToken);

                await _cache.SetAsync(key, task, TimeSpan.FromMinutes(5), cancellationToken);
            }

            if (task is null)
            {
                throw new NotFoundException($"Task {request.Id} was not found.");
            }

            return _mapper.Map<TaskItemDto>(task);
        }
    }
}
