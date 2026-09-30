using System.Diagnostics;
using FluentValidation;
using MediatR;
using Tracing.Common;
using Tracing.Metrics;

namespace Tracing.Behaviors;

/// <summary>
/// MediatR pipeline behavior: opens a span and collects metrics for the command currently being processed.
/// Registered before <see cref="ValidationBehavior{TRequest,TResponse}"/>, so it also sees rejected commands.
/// </summary>
public sealed class MetricsBehavior<TRequest, TResponse>(
    IAppMetrix<TRequest> metrix,
    ILogger<MetricsBehavior<TRequest, TResponse>> logger)
    : IPipelineBehavior<TRequest, TResponse>
    where TRequest : notnull
{
    public async Task<TResponse> Handle(
        TRequest request,
        RequestHandlerDelegate<TResponse> next,
        CancellationToken cancellationToken)
    {
        // The span is opened first and closed last: the timer below records while it is current,
        // so duration exemplars point to this span.
        using var activity = metrix.StartActivity();
        using var inProgress = metrix.TrackInProgress();
        using var timer = metrix.StartTimer();

        logger.LogInformation("Processing {Command}", metrix.Name);

        TResponse response;
        try
        {
            response = await next();
        }
        catch (ValidationException exception)
        {
            // A rejected command is the caller's mistake, not a failure of the service: the span status stays unset.
            activity?.SetTag("outcome", "rejected");
            metrix.Increment(tags: new TagList { { "outcome", "rejected" } });
            logger.LogWarning(
                "{Command} rejected: {Errors}",
                metrix.Name,
                string.Join("; ", exception.Errors.Select(e => e.ErrorMessage)));
            throw;
        }
        catch (Exception exception)
        {
            activity?.SetTag("outcome", "error");
            metrix.RecordException(exception);
            metrix.Increment(tags: new TagList { { "outcome", "error" } });
            logger.LogError(exception, "{Command} failed", metrix.Name);
            throw;
        }

        activity?.SetTag("outcome", "success");
        metrix.Increment(tags: new TagList { { "outcome", "success" } });

        if (response is DeliveryQuote quote)
        {
            activity?.SetTag("delivery.cost", (double)quote.Cost);
            metrix.Record((double)quote.Cost);
            logger.LogInformation(
                "{Command} quoted {DeliveryType} delivery: {Cost}, arrives {EstimatedArrival}",
                metrix.Name, quote.Type, quote.Cost, quote.EstimatedArrival);
        }

        return response;
    }
}
