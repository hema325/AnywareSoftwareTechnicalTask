namespace Application.Common.Authentication
{
    public record JwtToken
    {
        public string AccessToken { get; init; } = string.Empty;
        public string TokenType { get; init; } = "Bearer";
        public DateTime IssuedAt { get; init; }
        public DateTime ExpiresAt { get; init; }

        public int ExpiresIn => (int)(ExpiresAt - IssuedAt).TotalSeconds;
    }
}
