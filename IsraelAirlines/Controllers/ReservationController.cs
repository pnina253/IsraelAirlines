
using IsraelAirlines.api.Extensions;
using IsraelAirlines.core.Constants;
using IsraelAirlines.core.DTOs.Requests;
using IsraelAirlines.core.Interfaces.Services;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace IsraelAirlines.API.Controllers;

[Authorize]
[ApiController]
[Route("api/[controller]")]
public class ReservationController : ControllerBase
{
    private readonly IReservationService _reservationService;

    public ReservationController(IReservationService reservationService)
    {
        _reservationService = reservationService;
    }
    [Authorize(Roles = Roles.Admin)]
    [ProducesResponseType(StatusCodes.Status200OK)]
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
    [ProducesResponseType(StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status401Unauthorized)]
    [ProducesResponseType(StatusCodes.Status403Forbidden)]
    [ProducesResponseType(StatusCodes.Status404NotFound)]
    [HttpGet("{id:int}")]
    public async Task<IActionResult> GetById(
        int id,
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
    [ProducesResponseType(StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status401Unauthorized)]
    [ProducesResponseType(StatusCodes.Status403Forbidden)]
    [HttpGet("user/{userId:int}")]
    public async Task<IActionResult> GetByUserId(
        int userId,
        CancellationToken cancellationToken)
    {
        if (!User.IsInRole(Roles.Admin) &&
            (!User.TryGetUserId(out var currentUserId) ||
            currentUserId != userId))
        {
            return Forbid();
        }

        var reservations = await _reservationService.GetByUserIdAsync(
            userId, cancellationToken);

        return Ok(reservations);
    }
    [ProducesResponseType(StatusCodes.Status200OK)]
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
    [ProducesResponseType(StatusCodes.Status201Created)]
    [ProducesResponseType(StatusCodes.Status401Unauthorized)]
    [ProducesResponseType(StatusCodes.Status404NotFound)]
    [ProducesResponseType(StatusCodes.Status409Conflict)]
    [ProducesResponseType(StatusCodes.Status400BadRequest)]
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
}
