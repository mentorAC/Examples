using System.Diagnostics;
using FluentValidation;
using MediatR;
using Tracing.Metrics;

namespace Tracing.Behaviors;

/// <summary>
/// MediatR pipeline behavior: runs all FluentValidation validators for the request before the handler is invoked.
/// </summary>
public sealed class ValidationBehavior<TRequest, TResponse>(
    IEnumerable<IValidator<TRequest>> validators,
    IAppMetrix<TRequest> metrix)
    : IPipelineBehavior<TRequest, TResponse>
    where TRequest : notnull
{
    public async Task<TResponse> Handle(
        TRequest request,
        RequestHandlerDelegate<TResponse> next,
        CancellationToken cancellationToken)
    {
        if (!validators.Any())
        {
            return await next();
        }

        List<FluentValidation.Results.ValidationFailure> failures;

        // Only validation goes into this span; the handler becomes a sibling, not a child of it.
        using (var activity = metrix.StartActivity("validate"))
        {
            var context = new ValidationContext<TRequest>(request);
            var results = await Task.WhenAll(validators.Select(v => v.ValidateAsync(context, cancellationToken)));
            failures = results.SelectMany(r => r.Errors).ToList();

            activity?.SetTag("validation.errors", failures.Count);

            foreach (var failure in failures)
            {
                metrix.AddEvent("validation_failure", new TagList
                {
                    { "property", failure.PropertyName },
                    { "message", failure.ErrorMessage }
                });
            }
        }

        if (failures.Count > 0)
        {
            throw new ValidationException(failures);
        }

        return await next();
    }
}
