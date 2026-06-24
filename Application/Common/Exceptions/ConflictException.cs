namespace Application.Common.Exceptions
{
    public sealed class ConflictException : AppException
    {
        public ConflictException(string message) : base(message)
        {
        }

        public override int StatusCode => 409;
    }
}
