using Tracing.Common;
using Tracing.Metrics;

namespace Tracing.Features.ExpressDelivery;

public sealed class ExpressDeliveryHandler(
    IDeliveryStrategy<ExpressDeliveryCommand> strategy,
    TimeProvider timeProvider,
    IAppMetrix<ExpressDeliveryCommand> metrix,
    ILogger<ExpressDeliveryHandler> logger)
    : DeliveryQuoteHandler<ExpressDeliveryCommand>(strategy, timeProvider, metrix, logger);
