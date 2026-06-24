using Application.Common.Contracts;
using MediatR;
using Microsoft.Extensions.Logging;

namespace Application.Common.Behaviors
{
    internal class LoggingBehavior<TRequest, TResponse> : IPipelineBehavior<TRequest, TResponse>

    {
        private readonly ICurrentUser _currentUser;
        private readonly ILogger<LoggingBehavior<TRequest, TResponse>> _logger;

        public LoggingBehavior(ILogger<LoggingBehavior<TRequest, TResponse>> logger, ICurrentUser currentUser)
        {
            _logger = logger;
            _currentUser = currentUser;
        }

        public async Task<TResponse> Handle(TRequest request, RequestHandlerDelegate<TResponse> next, CancellationToken cancellationToken)
        {
            _logger.LogInformation("Handling {RequestName} for user {userId}", typeof(TRequest).Name, _currentUser.Id);

            var response = await next(cancellationToken);

            _logger.LogInformation("Handled {RequestName} for user {userId}", typeof(TRequest).Name, _currentUser.Id);

            return response;
        }
    }
}
