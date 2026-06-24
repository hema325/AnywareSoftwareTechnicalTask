using Application.Common.Contracts;
using Application.Common.Exceptions;
using Domain.Events;
using MediatR;
using Microsoft.EntityFrameworkCore;

namespace Application.TaskItems.Commands.UpdateTaskStatus
{
    internal sealed class UpdateTaskStatusCommandHandler : IRequestHandler<UpdateTaskStatusCommand>
    {
        private readonly IAppDbContext _context;
        private readonly ICurrentUser _currentUser;

        public UpdateTaskStatusCommandHandler(IAppDbContext context, ICurrentUser currentUser)
        {
            _context = context;
            _currentUser = currentUser;
        }

        public async Task Handle(UpdateTaskStatusCommand request, CancellationToken cancellationToken)
        {
            var task = await _context.TaskItems.FirstOrDefaultAsync(
                t => t.Id == request.Id && t.OwnerId == _currentUser.Id,
                cancellationToken);

            if (task is null)
            {
                throw new NotFoundException($"Task {request.Id} was not found.");
            }

            task.Status = request.Status;

            task.AddDomainEvent(new TaskItemUpdatedEvent(task));
            await _context.SaveChangesAsync(cancellationToken);
        }
    }
}
