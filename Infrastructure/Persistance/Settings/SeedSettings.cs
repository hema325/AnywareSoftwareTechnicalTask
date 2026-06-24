namespace Infrastructure.Persistance.Settings
{
    public class SeedSettings
    {
        public const string SectionName = "Seeds";

        public List<SeedUserSettings> Users { get; set; } = [];
        public List<SeedTaskItemSettings> TaskItems { get; set; } = [];
    }

    public class SeedUserSettings
    {
        public int Id { get; set; }
        public string Name { get; set; } = string.Empty;
        public string Email { get; set; } = string.Empty;
        public string Password { get; set; } = string.Empty;
        public UserRole Role { get; set; }
        public DateTime CreatedAt { get; set; }
    }

    public class SeedTaskItemSettings
    {
        public int Id { get; set; }
        public string Title { get; set; } = string.Empty;
        public string Description { get; set; } = string.Empty;
        public TaskItemStatus Status { get; set; } = TaskItemStatus.Pending;
        public TaskItemPriority Priority { get; set; }
        public DateTime CreatedAt { get; set; }
        public int OwnerId { get; set; }
    }
}
