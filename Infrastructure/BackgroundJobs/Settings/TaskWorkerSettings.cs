using System;
using System.Collections.Generic;
using System.Text;

namespace Infrastructure.BackgroundJobs.Settings
{
    public class TaskWorkerSettings
    {
        public const string SectionName = "TaskWorker";

        public TimeSpan PollInterval { get; set; } = TimeSpan.FromSeconds(10);
        public TimeSpan ProcessingTime { get; set; } = TimeSpan.FromSeconds(5);
    }
}
