using System.Diagnostics;
using MediatR;
using Tracing.Metrics;

namespace Tracing.Common;

/// <summary>
/// Strategy pattern context: the quote algorithm is written once and works only through the strategy interface.
/// </summary>
public abstract class DeliveryQuoteHandler<TCommand>(
    IDeliveryStrategy<TCommand> strategy,
    TimeProvider timeProvider,
    IAppMetrix<TCommand> metrix,
    ILogger logger) : IRequestHandler<TCommand, DeliveryQuote>
    where TCommand : IDeliveryCommand
{
    public Task<DeliveryQuote> Handle(TCommand request, CancellationToken cancellationToken)
    {
        using var activity = metrix.StartActivity("quote", tags: new TagList { { "delivery.type", strategy.Type.ToString() } });

        var today = DateOnly.FromDateTime(timeProvider.GetLocalNow().DateTime);

        decimal cost;
        using (metrix.StartActivity("calculate_cost"))
        {
            cost = Math.Round(strategy.CalculateCost(request), 2);
        }

        DateOnly arrival;
        using (metrix.StartActivity("estimate_arrival"))
        {
            arrival = strategy.EstimateArrival(request, today);
        }

        logger.LogDebug("{Strategy} calculated cost {Cost} and arrival {EstimatedArrival}", strategy.GetType().Name, cost, arrival);

        return Task.FromResult(new DeliveryQuote(strategy.Type, strategy.Description, cost, arrival));
    }
}
