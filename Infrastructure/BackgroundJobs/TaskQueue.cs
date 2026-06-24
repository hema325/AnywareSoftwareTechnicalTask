using System.Collections.Concurrent;
using Application.Common.Contracts;

namespace Infrastructure.BackgroundJobs
{
    internal sealed class TaskQueue : ITaskQueue
    {
        private readonly ConcurrentQueue<int> _queue = new();

        public void Enqueue(int taskId) => _queue.Enqueue(taskId);

        public bool TryDequeue(out int taskId) => _queue.TryDequeue(out taskId);
    }
}
