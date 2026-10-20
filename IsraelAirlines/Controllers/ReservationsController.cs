
using IsraelAirlines.API.Extensions;
using IsraelAirlines.core.Constants;
using IsraelAirlines.core.DTOs.Requests;
using IsraelAirlines.core.DTOs.Responses;
using IsraelAirlines.core.Interfaces.Services;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace IsraelAirlines.API.Controllers;

[Authorize]
[ApiController]
[Route("api/reservations")]
public class ReservationsController : ControllerBase
{
    private readonly IReservationService _reservationService;

    public ReservationsController(IReservationService reservationService)
    {
        _reservationService = reservationService;
    }

    [Authorize(Roles = Roles.Admin)]
    [ProducesResponseType(typeof(IReadOnlyList<ReservationResponse>), StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status401Unauthorized)]
    [ProducesResponseType(StatusCodes.Status403Forbidden)]
    [HttpGet]
    public async Task<IActionResult> GetAll(
        CancellationToken cancellationToken)
    {
        var reservations = await _reservationService.GetAllAsync(
            cancellationToken);

        return Ok(reservations);
    }

    [ProducesResponseType(typeof(ReservationResponse), StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status401Unauthorized)]
    [ProducesResponseType(StatusCodes.Status403Forbidden)]
    [ProducesResponseType(StatusCodes.Status404NotFound)]
    [HttpGet("{id:int}")]
    public async Task<IActionResult> GetById(
        [FromRoute] int id,
        CancellationToken cancellationToken)
    {
        var reservation = await _reservationService.GetByIdAsync(
            id, cancellationToken);

        if (reservation is null)
            return NotFound();

        if (!User.IsInRole(Roles.Admin) &&
            (!User.TryGetUserId(out var currentUserId) ||
            reservation.UserId != currentUserId))
        {
            return Forbid();
        }

        return Ok(reservation);
    }

    // The reservations of the signed-in user.
    [ProducesResponseType(typeof(IReadOnlyList<ReservationResponse>), StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status401Unauthorized)]
    [HttpGet("me")]
    public async Task<IActionResult> GetMine(
        CancellationToken cancellationToken)
    {
        if (!User.TryGetUserId(out var userId))
            return Unauthorized();

        var reservations = await _reservationService.GetByUserIdAsync(
            userId, cancellationToken);

        return Ok(reservations);
    }

    // 409 when the seat is already reserved, or was taken by another request at the same moment.
    [ProducesResponseType(typeof(ReservationResponse), StatusCodes.Status201Created)]
    [ProducesResponseType(typeof(ValidationProblemDetails), StatusCodes.Status400BadRequest)]
    [ProducesResponseType(StatusCodes.Status401Unauthorized)]
    [ProducesResponseType(StatusCodes.Status404NotFound)]
    [ProducesResponseType(typeof(ProblemDetails), StatusCodes.Status409Conflict)]
    [HttpPost]
    public async Task<IActionResult> Create(
        [FromBody] ReservationRequest request,
        CancellationToken cancellationToken)
    {
        if (!User.TryGetUserId(out var userId))
            return Unauthorized();

        var reservation = await _reservationService.CreateAsync(
            userId, request, cancellationToken);

        return CreatedAtAction(
            nameof(GetById),
            new { id = reservation.Id },
            reservation);
    }

    // Moves the reservation to another seat on the same flight.
    [ProducesResponseType(typeof(ReservationResponse), StatusCodes.Status200OK)]
    [ProducesResponseType(typeof(ValidationProblemDetails), StatusCodes.Status400BadRequest)]
    [ProducesResponseType(StatusCodes.Status401Unauthorized)]
    [ProducesResponseType(StatusCodes.Status403Forbidden)]
    [ProducesResponseType(StatusCodes.Status404NotFound)]
    [ProducesResponseType(typeof(ProblemDetails), StatusCodes.Status409Conflict)]
    [HttpPut("{id:int}")]
    public async Task<IActionResult> ChangeSeat(
        [FromRoute] int id,
        [FromBody] ReservationUpdateRequest request,
        CancellationToken cancellationToken)
    {
        if (!User.TryGetUserId(out var userId))
            return Unauthorized();

        var reservation = await _reservationService.ChangeSeatAsync(
            id, userId, User.IsInRole(Roles.Admin), request, cancellationToken);

        return Ok(reservation);
    }

    // Cancels the reservation and releases the seat.
    [ProducesResponseType(StatusCodes.Status204NoContent)]
    [ProducesResponseType(StatusCodes.Status401Unauthorized)]
    [ProducesResponseType(StatusCodes.Status403Forbidden)]
    [ProducesResponseType(StatusCodes.Status404NotFound)]
    [ProducesResponseType(typeof(ProblemDetails), StatusCodes.Status409Conflict)]
    [HttpDelete("{id:int}")]
    public async Task<IActionResult> Cancel(
        [FromRoute] int id,
        CancellationToken cancellationToken)
    {
        if (!User.TryGetUserId(out var userId))
            return Unauthorized();

        await _reservationService.CancelAsync(
            id, userId, User.IsInRole(Roles.Admin), cancellationToken);

        return NoContent();
    }
}
