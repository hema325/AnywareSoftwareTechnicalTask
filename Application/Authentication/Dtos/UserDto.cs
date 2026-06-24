namespace Application.Authentication.Dtos
{
    public record UserDto
    {
        public int Id { get; init; }
        public string Name { get; init; } = string.Empty;
        public string Email { get; init; } = string.Empty;
        public string Role { get; init; } = string.Empty;
        public DateTime CreatedAt { get; init; }
    }
}
