using Application.Common.Contracts;
using Application.TaskItems.Dtos;
using AutoMapper;
using MediatR;
using Microsoft.EntityFrameworkCore;

namespace Application.TaskItems.Queries.GetTasks
{
    internal sealed class GetTasksQueryHandler : IRequestHandler<GetTasksQuery, IReadOnlyList<TaskItemDto>>
    {
        private readonly IAppDbContext _context;
        private readonly ICurrentUser _currentUser;
        private readonly IMapper _mapper;

        public GetTasksQueryHandler(IAppDbContext context, ICurrentUser currentUser, IMapper mapper)
        {
            _context = context;
            _currentUser = currentUser;
            _mapper = mapper;
        }

        public async Task<IReadOnlyList<TaskItemDto>> Handle(GetTasksQuery request, CancellationToken cancellationToken)
        {
            var tasks = await _context.TaskItems
                .Where(t => t.OwnerId == _currentUser.Id)
                .OrderByDescending(t => t.Priority)
                .ThenBy(t => t.CreatedAt)
                .ToListAsync(cancellationToken);

            return _mapper.Map<IReadOnlyList<TaskItemDto>>(tasks);
        }
    }
}
