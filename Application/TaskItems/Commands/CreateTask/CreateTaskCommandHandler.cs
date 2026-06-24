using Application.Common.Contracts;
using Application.Common.Exceptions;
using MediatR;
using Microsoft.EntityFrameworkCore;

namespace Application.TaskItems.Commands.CreateTask
{
    internal sealed class CreateTaskCommandHandler : IRequestHandler<CreateTaskCommand, int>
    {
        private readonly IAppDbContext _context;
        private readonly ICurrentUser _currentUser;

        public CreateTaskCommandHandler(IAppDbContext context, ICurrentUser currentUser)
        {
            _context = context;
            _currentUser = currentUser;
        }

        public async Task<int> Handle(CreateTaskCommand request, CancellationToken cancellationToken)
        {
            var ownerId = _currentUser.Id!.Value;
            var today = DateTime.UtcNow.Date;

            var duplicateExists = await _context.TaskItems.AnyAsync(
                task => task.OwnerId == ownerId
                        && task.Title == request.Title
                        && task.CreatedAt.Date == today,
                cancellationToken);

            if (duplicateExists)
            {
                throw new ConflictException($"A task titled '{request.Title}' already exists for today.");
            }

            var task = new TaskItem
            {
                Title = request.Title,
                Description = request.Description,
                Priority = request.Priority,
                OwnerId = ownerId
            };

            _context.TaskItems.Add(task);
            await _context.SaveChangesAsync(cancellationToken);

            return task.Id;
        }
    }
}
