using Tracing.Common;
using Tracing.Metrics;

namespace Tracing.Features.PickupDelivery;

public sealed class PickupDeliveryHandler(
    IDeliveryStrategy<PickupDeliveryCommand> strategy,
    TimeProvider timeProvider,
    IAppMetrix<PickupDeliveryCommand> metrix,
    ILogger<PickupDeliveryHandler> logger)
    : DeliveryQuoteHandler<PickupDeliveryCommand>(strategy, timeProvider, metrix, logger);
