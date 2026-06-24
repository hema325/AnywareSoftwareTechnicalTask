using Application.Common.Exceptions;
using Microsoft.AspNetCore.Diagnostics;
using Microsoft.AspNetCore.Mvc;

namespace API.Common
{
    internal sealed class GlobalExceptionHandler : IExceptionHandler
    {
        public async ValueTask<bool> TryHandleAsync(
            HttpContext httpContext,
            Exception exception,
            CancellationToken cancellationToken)
        {
            var problem = new ProblemDetails
            {
                Status = (exception as AppException)?.StatusCode ?? StatusCodes.Status500InternalServerError,
                Title = exception is AppException ? exception.Message : "An unexpected error occurred"
            };

            if (exception is ValidationException validationException)
            {
                problem.Extensions["errors"] = validationException.Errors;
            }

            httpContext.Response.StatusCode = problem.Status.Value;
            await httpContext.Response.WriteAsJsonAsync(problem, cancellationToken);

            return true;
        }
    }
}
