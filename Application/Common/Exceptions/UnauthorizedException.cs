namespace Application.Common.Exceptions
{
    public sealed class UnauthorizedException : AppException
    {
        public UnauthorizedException(string message) : base(message)
        {
        }

        public override int StatusCode => 401;
    }
}
