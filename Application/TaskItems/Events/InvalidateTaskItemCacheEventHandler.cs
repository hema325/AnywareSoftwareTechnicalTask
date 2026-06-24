using Application.Common.Contracts;
using Domain.Events;
using MediatR;
using Microsoft.Extensions.Logging;

namespace Application.TaskItems.Events
{
    internal class InvalidateTaskItemCacheEventHandler : 
        INotificationHandler<TaskItemUpdatedEvent>,
        INotificationHandler<TaskItemDeletedEvent>
    {
        private readonly ICache _cache;
        private readonly ILogger<InvalidateTaskItemCacheEventHandler> _logger;

        public InvalidateTaskItemCacheEventHandler(ICache cache, ILogger<InvalidateTaskItemCacheEventHandler> logger)
        {
            _cache = cache;
            _logger = logger;
        }

        public async Task Handle(TaskItemUpdatedEvent notification, CancellationToken cancellationToken)
        {
            await RemoveTaskFromCache(notification.Task, cancellationToken);
        }
        public async Task Handle(TaskItemDeletedEvent notification, CancellationToken cancellationToken)
        {
            await RemoveTaskFromCache(notification.Task, cancellationToken);
        }

        private async Task RemoveTaskFromCache(TaskItem task, CancellationToken cancellationToken)
        {
            var key = $"{nameof(TaskItem)}_{task.Id}";
            await _cache.RemoveAsync(key, cancellationToken);
            _logger.LogInformation("Task {TaskId} updated and cache invalidated for key {CacheKey}", task.Id, key);
        }
    }
}
