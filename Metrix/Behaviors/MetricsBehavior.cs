using System.Diagnostics;
using FluentValidation;
using MediatR;
using Metrix.Common;
using Metrix.Metrics;

namespace Metrix.Behaviors;

/// <summary>
/// MediatR pipeline behavior: collects metrics for the command currently being processed.
/// Registered before <see cref="ValidationBehavior{TRequest,TResponse}"/>, so it also sees rejected commands.
/// </summary>
public sealed class MetricsBehavior<TRequest, TResponse>(IAppMetrix<TRequest> metrix)
    : IPipelineBehavior<TRequest, TResponse>
    where TRequest : notnull
{
    public async Task<TResponse> Handle(
        TRequest request,
        RequestHandlerDelegate<TResponse> next,
        CancellationToken cancellationToken)
    {
        using var inProgress = metrix.TrackInProgress();
        using var timer = metrix.StartTimer();

        TResponse response;
        try
        {
            response = await next();
        }
        catch (ValidationException)
        {
            metrix.Increment(tags: new TagList { { "outcome", "rejected" } });
            throw;
        }
        catch
        {
            metrix.Increment(tags: new TagList { { "outcome", "error" } });
            throw;
        }

        metrix.Increment(tags: new TagList { { "outcome", "success" } });

        if (response is DeliveryQuote quote)
        {
            metrix.Record((double)quote.Cost);
        }

        return response;
    }
}
