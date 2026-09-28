using Strategy.Common;

namespace Strategy.Features.ExpressDelivery;

public sealed class ExpressDeliveryHandler(
    IDeliveryStrategy<ExpressDeliveryCommand> strategy,
    TimeProvider timeProvider) : DeliveryQuoteHandler<ExpressDeliveryCommand>(strategy, timeProvider);
