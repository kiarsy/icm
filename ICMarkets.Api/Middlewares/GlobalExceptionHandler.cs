using FluentValidation;
using ICMarkets.Domain.Common.Exceptions;
using Microsoft.AspNetCore.Diagnostics;
using Microsoft.AspNetCore.Mvc;

namespace ICMarkets.Api.Middlewares;

public class GlobalExceptionHandler(
    IProblemDetailsService problemDetailsService,
    IHostEnvironment environment,
    ILogger<GlobalExceptionHandler> logger)
    : IExceptionHandler
{
    public async ValueTask<bool> TryHandleAsync(
        HttpContext httpContext, Exception exception, CancellationToken cancellationToken)
    {
        var (status, title, errors) = ExtractInfo(exception, environment);

        if (status >= StatusCodes.Status500InternalServerError)
        {
            logger.LogError(exception, "Unhandled exception processing {Path}.", httpContext.Request.Path);
        }
        else
        {
            logger.LogWarning("Request to {Path} failed: {Title}.", httpContext.Request.Path, title);
        }

        httpContext.Response.StatusCode = status;

        var problemDetails = new ProblemDetails
        {
            Status = status,
            Title = title,
            Instance = httpContext.Request.Path
        };

        if (errors is not null)
        {
            problemDetails.Extensions["errors"] = errors;
        }

        return await problemDetailsService.TryWriteAsync(new ProblemDetailsContext
        {
            HttpContext = httpContext,
            Exception = exception,
            ProblemDetails = problemDetails
        });
    }

    private static (int Status, string Title, IDictionary<string, string[]>? Errors) ExtractInfo(Exception exception,
        IHostEnvironment environment)
        => exception switch
        {
            BaseException ex => (
                ex.HttpCode,
                ex.Message,
                null
            ),
            ValidationException ve => (
                StatusCodes.Status400BadRequest,
                "One or more validation errors occurred.",
                ve.Errors
                    .GroupBy(e => e.PropertyName)
                    .ToDictionary(g => g.Key, g => g.Select(e => e.ErrorMessage).Distinct().ToArray())),
            _ => (
                StatusCodes.Status500InternalServerError,
                "An unexpected error occurred.",
                environment.IsDevelopment()
                    ? new Dictionary<string, string[]>
                    {
                        ["message"] = [exception.Message],
                        ["type"] = [exception.GetType().FullName ?? exception.GetType().Name],
                        ["stackTrace"] = [exception.StackTrace ?? string.Empty]
                    }
                    : null
            )
        };
}