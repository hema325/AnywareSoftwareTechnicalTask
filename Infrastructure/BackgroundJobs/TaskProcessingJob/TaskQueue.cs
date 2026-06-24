using Application.Common.Contracts;
using MediatR;
using Microsoft.Extensions.Logging;
using System.Collections.Concurrent;

namespace Infrastructure.BackgroundJobs.TaskProcessingJob
{
    internal sealed class TaskQueue : ITaskQueue
    {
        private readonly ConcurrentQueue<int> _queue;
        private readonly ILogger<TaskQueue> _logger;

        public TaskQueue(ILogger<TaskQueue> logger)
        {
            _queue = new();
            _logger = logger;
        }

        public void Enqueue(int taskId)
        {
            _queue.Enqueue(taskId);
            _logger.LogInformation("Task {TaskId} queued for processing", taskId);
        }

        public bool TryDequeue(out int taskId)
        {
            var result = _queue.TryDequeue(out taskId);
            _logger.LogInformation("Task {TaskId} dequeued for processing", taskId);
            return result;
        }
    }
}
