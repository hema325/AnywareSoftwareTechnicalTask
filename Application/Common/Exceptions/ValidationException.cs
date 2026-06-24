namespace Application.Common.Exceptions
{
    public sealed class ValidationException : AppException
    {
        public ValidationException(IReadOnlyDictionary<string, string[]> errors)
            : base("One or more validation errors occurred.")
        {
            Errors = errors;
        }

        public override int StatusCode => 400;

        public IReadOnlyDictionary<string, string[]> Errors { get; }
    }
}
