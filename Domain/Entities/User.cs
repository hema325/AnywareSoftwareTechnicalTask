namespace Domain.Entities
{
    public class User: EntityBase
    {
        public string Name { get; set; }
        public string Email { get; set; }   
        public string HashedPassword { get; set; }
        public UserRole Role { get; set; }
        public DateTime CreatedAt { get; set; } = DateTime.UtcNow;

        public ICollection<TaskItem> TaskItems { get; set; }
    }
}
