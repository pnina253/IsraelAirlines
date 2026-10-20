
using IsraelAirlines.core.Constants;
using IsraelAirlines.core.DTOs.Requests;
using IsraelAirlines.core.Interfaces.Services;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace IsraelAirlines.API.Controllers;

[ApiController]
[Route("api/[controller]")]
public class FlightController : ControllerBase
{
    private readonly IFlightService _flightService;

    public FlightController(IFlightService flightService)
    {
        _flightService = flightService;
    }
    [ProducesResponseType(StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status400BadRequest)]
    [HttpGet]
    [HttpGet("search")]
    public async Task<IActionResult> GetAll(
        [FromQuery] FlightSearchQuery query,
        CancellationToken cancellationToken)
    {
        var flights = await _flightService.SearchAsync(
            query, cancellationToken);

        return Ok(flights);
    }
    [ProducesResponseType(StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status404NotFound)]
    [HttpGet("{id:int}")]
    public async Task<IActionResult> GetById(
        int id,
        CancellationToken cancellationToken)
    {
        var flight = await _flightService.GetByIdAsync(
            id, cancellationToken);

        if (flight is null)
            return NotFound();

        return Ok(flight);
    }
    // Only seat numbers are returned, never who reserved them.
    [Authorize]
    [ProducesResponseType(StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status401Unauthorized)]
    [ProducesResponseType(StatusCodes.Status404NotFound)]
    [HttpGet("{id:int}/reserved-seats")]
    public async Task<IActionResult> GetReservedSeats(
        int id,
        CancellationToken cancellationToken)
    {
        var seats = await _flightService.GetReservedSeatsAsync(
            id, cancellationToken);

        if (seats is null)
            return NotFound();

        return Ok(seats);
    }
    [Authorize(Roles = Roles.Admin)]
    [ProducesResponseType(StatusCodes.Status201Created)]
    [ProducesResponseType(StatusCodes.Status400BadRequest)]
    [ProducesResponseType(StatusCodes.Status401Unauthorized)]
    [ProducesResponseType(StatusCodes.Status403Forbidden)]
    [ProducesResponseType(StatusCodes.Status404NotFound)]
    [ProducesResponseType(StatusCodes.Status409Conflict)]
    [HttpPost]
    public async Task<IActionResult> Create(
        [FromBody] FlightRequest request,
        CancellationToken cancellationToken)
    {
        var flight = await _flightService.CreateAsync(
            request, cancellationToken);

        return CreatedAtAction(
         nameof(GetById),
         new { id = flight.Id },
         flight);
    }
}
