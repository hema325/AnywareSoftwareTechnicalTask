namespace Application.Common.Contracts
{
    public interface ICurrentUser
    {
        int? Id { get; }
        string? Email { get; }
        string? Role { get; }
    }
}
