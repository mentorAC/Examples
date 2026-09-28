using MediatR;
using Metrix.Common;

namespace Metrix.Features.StandardDelivery;

public sealed class StandardDeliveryHandler(
    IDeliveryStrategy<StandardDeliveryCommand> strategy,
    TimeProvider timeProvider) : IRequestHandler<StandardDeliveryCommand, DeliveryQuote>
{
    public Task<DeliveryQuote> Handle(StandardDeliveryCommand request, CancellationToken cancellationToken)
    {
        var today = DateOnly.FromDateTime(timeProvider.GetLocalNow().DateTime);

        var quote = new DeliveryQuote(
            strategy.Type,
            strategy.Description,
            Math.Round(strategy.CalculateCost(request), 2),
            strategy.EstimateArrival(request, today));

        return Task.FromResult(quote);
    }
}
