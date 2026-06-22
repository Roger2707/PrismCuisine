using Microsoft.AspNetCore.Diagnostics;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using PrismERP.BuildingBlocks.Domain.Exceptions;

namespace PrismERP.Api.Middlewares
{
    public class GlobalExceptionHandler : IExceptionHandler
    {
        private readonly ILogger<GlobalExceptionHandler> _logger;
        public GlobalExceptionHandler(ILogger<GlobalExceptionHandler> logger)
        {
            _logger = logger;
        }

        public async ValueTask<bool> TryHandleAsync(HttpContext httpContext, Exception exception, CancellationToken cancellationToken)
        {
            // Client disconnected mid-request — not a server fault; skip error logging and response body
            if (exception is OperationCanceledException or TaskCanceledException)
            {
                _logger.LogInformation("Request cancelled by client: {Path}", httpContext.Request.Path);
                return true;
            }

            var (statusCode, response) = exception switch
            {
                // 409 Conflict for explicit business conflicts (e.g. double-approve same SO)
                ConflictException ex => (
                    StatusCodes.Status409Conflict,
                    new ProblemDetails
                    {
                        Type = "conflict",
                        Title = ex.Message,
                        Status = StatusCodes.Status409Conflict
                    }
                ),
                // 409 Conflict for optimistic concurrency — exhausted retries
                DbUpdateConcurrencyException => (
                    StatusCodes.Status409Conflict,
                    new ProblemDetails
                    {
                        Type = "conflict",
                        Title = "Data was modified by another user. Please refresh and try again.",
                        Status = StatusCodes.Status409Conflict
                    }
                ),
                // 400 Bad Request for validation errors
                ValidationException ex => (
                    StatusCodes.Status400BadRequest,
                    new ProblemDetails
                    {
                        Type = "validation-error",
                        Title = "Validation Failed",
                        Status = StatusCodes.Status400BadRequest,
                        Extensions = { ["errors"] = ex.Errors }
                    }
                ),
                // 404 Not Found for missing resources
                NotFoundException ex => (
                    StatusCodes.Status404NotFound,
                    new ProblemDetails
                    {
                        Type = "not-found",
                        Title = ex.Message,
                        Status = StatusCodes.Status404NotFound
                    }
                ),
                // 422 Unprocessable Entity for business rule violations
                BusinessException ex => (
                    StatusCodes.Status422UnprocessableEntity,
                    new ProblemDetails
                    {
                        Type = "business-error",
                        Title = ex.Message,
                        Status = StatusCodes.Status422UnprocessableEntity,
                        Extensions = { ["code"] = ex.Code }
                    }
                ),
                // 401 Unauthorized for authentication failures
                UnauthorizedAccessException => (
                    StatusCodes.Status401Unauthorized,
                    new ProblemDetails
                    {
                        Type = "unauthorized",
                        Title = "Unauthorized",
                        Status = StatusCodes.Status401Unauthorized
                    }
                ),
                // 403 Forbidden exception
                ForbiddenException => (
                    StatusCodes.Status403Forbidden,
                    new ProblemDetails
                    {
                        Type = "forbidden",
                        Title = "Forbidden",
                        Status = StatusCodes.Status403Forbidden
                    }
                ),
                // 500 Internal Server Error for unhandled exceptions
                _ => (
                    StatusCodes.Status500InternalServerError,
                    new ProblemDetails
                    {
                        Type = "server-error",
                        Title = "An unexpected error occurred.",
                        Status = StatusCodes.Status500InternalServerError
                    }
                )
            };

            _logger.LogError(exception, "Exception caught: {Message}", exception.Message);

            httpContext.Response.StatusCode = statusCode;
            await httpContext.Response.WriteAsJsonAsync(response, cancellationToken);

            return true;
        }
    }
}
