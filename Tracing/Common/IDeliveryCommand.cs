using MediatR;

namespace Tracing.Common;

/// <summary>
/// Marker for delivery quote commands: every such command returns a <see cref="DeliveryQuote"/>.
/// </summary>
public interface IDeliveryCommand : IRequest<DeliveryQuote>;
