using Application.Common.Contracts;
using Application.Common.Exceptions;
using Domain.Events;
using MediatR;
using Microsoft.EntityFrameworkCore;

namespace Application.TaskItems.Commands.DeleteTask
{
    internal sealed class DeleteTaskCommandHandler : IRequestHandler<DeleteTaskCommand>
    {
        private readonly IAppDbContext _context;
        private readonly ICurrentUser _currentUser;

        public DeleteTaskCommandHandler(IAppDbContext context, ICurrentUser currentUser)
        {
            _context = context;
            _currentUser = currentUser;
        }

        public async Task Handle(DeleteTaskCommand request, CancellationToken cancellationToken)
        {
            var task = await _context.TaskItems.FirstOrDefaultAsync(
                t => t.Id == request.Id && t.OwnerId == _currentUser.Id,
                cancellationToken);

            if (task is null)
            {
                throw new NotFoundException($"Task {request.Id} was not found.");
            }

            task.AddDomainEvent(new TaskItemDeletedEvent(task));
            _context.TaskItems.Remove(task);
            await _context.SaveChangesAsync(cancellationToken);
        }
    }
}
