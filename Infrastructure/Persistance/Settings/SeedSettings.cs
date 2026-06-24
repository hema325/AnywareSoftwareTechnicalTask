namespace Infrastructure.Persistance.Settings
{
    public class SeedSettings
    {
        public const string SectionName = "Seeds";

        public List<SeedUserSettings> Users { get; set; } = [];
    }

    public class SeedUserSettings
    {
        public string Name { get; set; } = string.Empty;
        public string Email { get; set; } = string.Empty;
        public string Password { get; set; } = string.Empty;
        public UserRole Role { get; set; }
    }
}
