namespace Domain.Entities
{
    public class TaskItem: EntityBase
    {
        public string Title { get; set; }
        public string Description { get; set; }
        public TaskItemStatus Status { get; set; } = TaskItemStatus.Pending;
        public TaskItemPriority Priority { get; set; } = TaskItemPriority.Low;
        public DateTime CreatedAt { get; set; } = DateTime.UtcNow;

        public int OwnerId { get; set; }
        public User Owner { get; set; }
    }
}
