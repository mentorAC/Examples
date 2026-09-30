using Tracing.Common;

namespace Tracing.Features.PickupDelivery;

public sealed record PickupDeliveryCommand(
    string PickupPointId,
    decimal WeightKg) : IDeliveryCommand;
