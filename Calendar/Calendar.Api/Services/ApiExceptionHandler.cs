using Microsoft.AspNetCore.Diagnostics;
using Microsoft.AspNetCore.Mvc;

namespace Calendar.Api.Services
{
    public class ApiExceptionHandler : IExceptionHandler
    {
        private readonly IProblemDetailsService _problemDetails;
        private readonly ILogger<ApiExceptionHandler> _logger;

        public ApiExceptionHandler(IProblemDetailsService problemDetails, ILogger<ApiExceptionHandler> logger)
        {
            _problemDetails = problemDetails;
            _logger = logger;
        }

        public async ValueTask<bool> TryHandleAsync(
            HttpContext httpContext, Exception exception, CancellationToken cancellationToken)
        {
            if (exception is not ApiException apiException)
            {
                return false;
            }

            _logger.LogInformation("Запрос отклонён: {StatusCode} {Title}", apiException.StatusCode, apiException.Message);

            ProblemDetails problem = apiException.Errors is null
                ? new ProblemDetails()
                : new ValidationProblemDetails(apiException.Errors);
            problem.Status = apiException.StatusCode;
            problem.Title = apiException.Message;

            httpContext.Response.StatusCode = apiException.StatusCode;
            return await _problemDetails.TryWriteAsync(new ProblemDetailsContext
            {
                HttpContext = httpContext,
                ProblemDetails = problem
            });
        }
    }
}
