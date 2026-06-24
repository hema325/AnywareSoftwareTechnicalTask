using Application.Common.Contracts;
using Domain.Events;
using MediatR;
using Microsoft.Extensions.Logging;

namespace Application.TaskItems.Events
{
    internal class InvalidateTaskItemCacheEventHandler : INotificationHandler<TaskItemUpdatedEvent>
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
            var key = $"{nameof(TaskItem)}_{notification.Id}";
            await _cache.RemoveAsync(key, cancellationToken);
            _logger.LogInformation("Task {TaskId} updated and cache invalidated for key {CacheKey}", notification.Id, key);
        }
    }
}
