using Metrix.Common;

namespace Metrix.Features.ExpressDelivery;

public sealed class ExpressDeliveryHandler(
    IDeliveryStrategy<ExpressDeliveryCommand> strategy,
    TimeProvider timeProvider) : DeliveryQuoteHandler<ExpressDeliveryCommand>(strategy, timeProvider);
