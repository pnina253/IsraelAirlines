
using IsraelAirlines.core.DTOs.Requests;
using IsraelAirlines.core.Interfaces;
using IsraelAirlines.core.Interfaces.Services;
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
    [HttpGet]
public async Task<IActionResult> GetAll(
    CancellationToken cancellationToken)
    {
        var flights = await _flightService.SearchAsync(
            null, null, cancellationToken);

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
    [ProducesResponseType(StatusCodes.Status200OK)]
    [HttpGet("search")]
    public async Task<IActionResult> Search(
        [FromQuery] string? origin,
        [FromQuery] string? destination,
        CancellationToken cancellationToken)
    {
        var flights = await _flightService.SearchAsync(
            origin, destination, cancellationToken);

        return Ok(flights);
    }
    [ProducesResponseType(StatusCodes.Status201Created)]
    [ProducesResponseType(StatusCodes.Status400BadRequest)]
    [ProducesResponseType(StatusCodes.Status404NotFound)]
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
