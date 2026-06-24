using Application.Common.Contracts;
using Domain.Events;
using MediatR;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Logging;

namespace Application.TaskItems.Events
{
    internal class ReQueueTaskItemForProcessingEventHandler : INotificationHandler<TaskItemUpdatedEvent>    
    {
        private readonly ITaskQueue _taskQueue;
        private readonly ILogger<QueueTaskItemForProcessingEventHandler> _logger;
        private readonly IAppDbContext _context;

        public ReQueueTaskItemForProcessingEventHandler(ITaskQueue taskQueue, ILogger<QueueTaskItemForProcessingEventHandler> logger, IAppDbContext context)
        {
            _taskQueue = taskQueue;
            _logger = logger;
            _context = context;
        }

        public async Task Handle(TaskItemUpdatedEvent notification, CancellationToken cancellationToken)
        {
            var isPending = await _context.TaskItems
                .AnyAsync(task => task.Id == notification.Id && task.Status == TaskItemStatus.Pending);
            
            if (isPending)
            {
                _taskQueue.Enqueue(notification.Id);
            }
        }
    }
}
