using FluentValidation;
using ICMarkets.Application.Abstractions;
using MediatR;
using Microsoft.Extensions.Logging;

namespace ICMarkets.Application.Common.Behaviors;

public sealed class MediaRPipelineBehavior<TRequest, TResponse>(
    IEnumerable<IValidator<TRequest>> validators,
    ILogger<MediaRPipelineBehavior<TRequest, TResponse>> logger)
    : IPipelineBehavior<TRequest, TResponse>
    where TRequest : notnull
{
    public async Task<TResponse> Handle(
        TRequest request, RequestHandlerDelegate<TResponse> next, CancellationToken cancellationToken)
    {
        if (!validators.Any())
        {
            return await HandleWithRetry(request, next);
        }

        var context = new ValidationContext<TRequest>(request);
        var results = await Task.WhenAll(
            validators.Select(v => v.ValidateAsync(context, cancellationToken)));

        var failures = results
            .SelectMany(r => r.Errors)
            .Where(f => f is not null)
            .ToList();

        if (failures.Count != 0)
        {
            throw new ValidationException(failures);
        }

        return await HandleWithRetry(request, next);
    }

    private async Task<TResponse> HandleWithRetry(TRequest request, RequestHandlerDelegate<TResponse> next)
    {
        if (request is IRequestRetriable)
        {
            const int maxAttempts = 2;
            for (var attempt = 1;; attempt++)
            {
                try
                {
                    return await next();
                }
                catch (Exception) when (attempt < maxAttempts)
                {
                    logger.LogWarning("Exception on handling {request}, retry {Attempt}/{Max}",
                        request.GetType().Name, attempt, maxAttempts);
                }
            }
        }

        return await next();
    }
}