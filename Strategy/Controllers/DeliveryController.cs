using MediatR;
using Microsoft.AspNetCore.Mvc;
using Strategy.Common;
using Strategy.Features.ExpressDelivery;
using Strategy.Features.PickupDelivery;
using Strategy.Features.StandardDelivery;

namespace Strategy.Controllers;

[ApiController]
[Route("api/delivery")]
[Produces("application/json")]
[ProducesResponseType<DeliveryQuote>(StatusCodes.Status200OK)]
[ProducesResponseType<HttpValidationProblemDetails>(StatusCodes.Status400BadRequest, "application/problem+json")]
public sealed class DeliveryController(ISender sender) : ControllerBase
{
    [HttpPost("standard")]
    public Task<DeliveryQuote> Standard(StandardDeliveryCommand command, CancellationToken ct) =>
        sender.Send(command, ct);

    [HttpPost("express")]
    public Task<DeliveryQuote> Express(ExpressDeliveryCommand command, CancellationToken ct) =>
        sender.Send(command, ct);

    [HttpPost("pickup")]
    public Task<DeliveryQuote> Pickup(PickupDeliveryCommand command, CancellationToken ct) =>
        sender.Send(command, ct);
}
