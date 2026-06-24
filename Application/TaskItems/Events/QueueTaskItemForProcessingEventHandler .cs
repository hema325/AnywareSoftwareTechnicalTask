using Application.Common.Contracts;
using Domain.Events;
using MediatR;
using Microsoft.Extensions.Logging;

namespace Application.TaskItems.Events
{
    internal class QueueTaskItemForProcessingEventHandler : INotificationHandler<TaskItemCreatedEvent>
    {
        private readonly ITaskQueue _taskQueue;
        private readonly ILogger<QueueTaskItemForProcessingEventHandler> _logger;
        private readonly IAppDbContext _context;

        public QueueTaskItemForProcessingEventHandler(ITaskQueue taskQueue, ILogger<QueueTaskItemForProcessingEventHandler> logger, IAppDbContext context)
        {
            _taskQueue = taskQueue;
            _logger = logger;
            _context = context;
        }

        public async Task Handle(TaskItemCreatedEvent notification, CancellationToken cancellationToken)
        {
            _taskQueue.Enqueue(notification.Task.Id);
        }
    }
}
