namespace Domain.Entities
{
    public class User: AuditableEntity
    {
        public string Name { get; set; }
        public string Email { get; set; }   
        public string HashedPassword { get; set; }
        public UserRole Role { get; set; }

        public ICollection<TaskItem> TaskItems { get; set; }
    }
}
