using System.Diagnostics;
using MediatR;
using Tracing.Common;
using Tracing.Metrics;

namespace Tracing.Features.StandardDelivery;

public sealed class StandardDeliveryHandler(
    IDeliveryStrategy<StandardDeliveryCommand> strategy,
    TimeProvider timeProvider,
    IAppMetrix<StandardDeliveryCommand> metrix,
    ILogger<StandardDeliveryHandler> logger) : IRequestHandler<StandardDeliveryCommand, DeliveryQuote>
{
    public Task<DeliveryQuote> Handle(StandardDeliveryCommand request, CancellationToken cancellationToken)
    {
        using var activity = metrix.StartActivity("quote", tags: new TagList
        {
            { "delivery.type", strategy.Type.ToString() },
            { "parcel.weight_kg", (double)request.WeightKg },
            { "parcel.distance_km", request.DistanceKm }
        });

        var today = DateOnly.FromDateTime(timeProvider.GetLocalNow().DateTime);

        decimal cost;
        using (metrix.StartActivity("calculate_cost"))
        {
            cost = Math.Round(strategy.CalculateCost(request), 2);
        }

        if (request.IsFragile)
        {
            metrix.AddEvent("fragile_surcharge_applied");
        }

        var quote = new DeliveryQuote(strategy.Type, strategy.Description, cost, strategy.EstimateArrival(request, today));

        logger.LogDebug("Standard delivery calculated cost {Cost} and arrival {EstimatedArrival}", quote.Cost, quote.EstimatedArrival);

        return Task.FromResult(quote);
    }
}
