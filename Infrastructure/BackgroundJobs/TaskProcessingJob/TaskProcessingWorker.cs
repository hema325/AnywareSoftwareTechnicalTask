using Domain.Events;
using Infrastructure.BackgroundJobs.TaskProcessingJob.Settings;
using Infrastructure.Persistance;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;

namespace Infrastructure.BackgroundJobs.TaskProcessingJob
{
    internal sealed class TaskProcessingWorker : BackgroundService
    {
        private readonly IServiceScopeFactory _scopeFactory;
        private readonly ITaskQueue _taskQueue;
        private readonly TaskWorkerSettings _settings;
        private readonly ILogger<TaskProcessingWorker> _logger;

        public TaskProcessingWorker(IServiceScopeFactory scopeFactory, ILogger<TaskProcessingWorker> logger, ITaskQueue taskQueue, IOptions<TaskWorkerSettings> options)
        {
            _scopeFactory = scopeFactory;
            _logger = logger;
            _taskQueue = taskQueue;
            _settings = options.Value;
        }

        protected override async Task ExecuteAsync(CancellationToken stoppingToken)
        {
            while (!stoppingToken.IsCancellationRequested)
            {
                await ProcessTaskAsync(stoppingToken);
                await Task.Delay(_settings.PollInterval, stoppingToken);
            }
        }

        private async Task ProcessTaskAsync(CancellationToken stoppingToken)
        {
            if(!_taskQueue.TryDequeue(out var taskId))
            {
                _logger.LogInformation("No tasks to process.");
                return;
            }
        
            using var scope = _scopeFactory.CreateScope();
            var context = scope.ServiceProvider.GetRequiredService<AppDbContext>();

           var task = await context.TaskItems
                .Where(task => task.Id == taskId)
                .FirstOrDefaultAsync(stoppingToken);

            if(task == null)
            {
                _logger.LogWarning("Task {TaskId} not found in the database.", taskId);
                return;
            }

            task.Status = TaskItemStatus.InProgress;
            task.AddDomainEvent(new TaskItemUpdatedEvent(task.Id));
            await context.SaveChangesAsync(stoppingToken);
            _logger.LogInformation("Started processing task {TaskId} ({Title}).", task.Id, task.Title);

            // Simulate task processing
            await Task.Delay(_settings.ProcessingTime, stoppingToken);

            task.Status = TaskItemStatus.Done;
            task.AddDomainEvent(new TaskItemUpdatedEvent(task.Id));
            await context.SaveChangesAsync(stoppingToken);
            _logger.LogInformation("Finished task {TaskId} ({Title}).", task.Id, task.Title);
        }
    }
}
