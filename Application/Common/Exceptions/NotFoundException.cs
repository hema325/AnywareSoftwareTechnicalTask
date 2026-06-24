namespace Application.Common.Exceptions
{
    public sealed class NotFoundException : AppException
    {
        public NotFoundException(string message) : base(message)
        {
        }

        public override int StatusCode => 404;
    }
}
