using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.WebUtilities;

namespace API.Common
{
    public static class ErrorHandlingExtensions
    {
        public static IApplicationBuilder UseErrorHandling(this IApplicationBuilder app)
        {
            app.UseExceptionHandler();

            app.UseStatusCodePages(async statusCodeContext =>
            {
                var response = statusCodeContext.HttpContext.Response;

                var problem = new ProblemDetails
                {
                    Status = response.StatusCode,
                    Title = ReasonPhrases.GetReasonPhrase(response.StatusCode)
                };

                await response.WriteAsJsonAsync(problem);
            });

            return app;
        }
    }
}
