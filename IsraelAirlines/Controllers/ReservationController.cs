
using IsraelAirlines.core.DTOs.Requests;
using IsraelAirlines.core.Interfaces;
using IsraelAirlines.core.Interfaces.Services;
using Microsoft.AspNetCore.Mvc;

namespace IsraelAirlines.API.Controllers;

[ApiController]
[Route("api/[controller]")]
public class ReservationController : ControllerBase
{
    private readonly IReservationService _reservationService;

    public ReservationController(IReservationService reservationService)
    {
        _reservationService = reservationService;
    }
    [ProducesResponseType(StatusCodes.Status200OK)]
    [HttpGet]
    public async Task<IActionResult> GetAll(
    CancellationToken cancellationToken)
    {
        var reservations = await _reservationService.GetAllAsync(
            cancellationToken);

        return Ok(reservations);
    }
    [ProducesResponseType(StatusCodes.Status200OK)]
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

        return Ok(reservation);
    }
    [ProducesResponseType(StatusCodes.Status200OK)]
    [HttpGet("user/{userId:int}")]
    public async Task<IActionResult> GetByUserId(
        int userId,
        CancellationToken cancellationToken)
    {
        var reservations = await _reservationService.GetByUserIdAsync(
            userId, cancellationToken);

        return Ok(reservations);
    }
    [ProducesResponseType(StatusCodes.Status201Created)]
    [ProducesResponseType(StatusCodes.Status404NotFound)]
    [ProducesResponseType(StatusCodes.Status409Conflict)]
    [ProducesResponseType(StatusCodes.Status400BadRequest)]
    [HttpPost]
    public async Task<IActionResult> Create(
        [FromBody] ReservationRequest request,
        CancellationToken cancellationToken)
    {
        var reservation = await _reservationService.CreateAsync(
            request, cancellationToken);

        return CreatedAtAction(
            nameof(GetById),
            new { id = reservation.Id },
            reservation);
    }
    }
