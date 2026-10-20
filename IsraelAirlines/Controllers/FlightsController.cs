
using IsraelAirlines.core.Constants;
using IsraelAirlines.core.DTOs.Requests;
using IsraelAirlines.core.DTOs.Responses;
using IsraelAirlines.core.Interfaces.Services;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace IsraelAirlines.API.Controllers;

[ApiController]
[Route("api/flights")]
public class FlightsController : ControllerBase
{
    private readonly IFlightService _flightService;

    public FlightsController(IFlightService flightService)
    {
        _flightService = flightService;
    }

    // Filtering, sorting and paging all run in the database.
    [ProducesResponseType(typeof(PagedResult<FlightResponse>), StatusCodes.Status200OK)]
    [ProducesResponseType(typeof(ValidationProblemDetails), StatusCodes.Status400BadRequest)]
    [HttpGet]
    public async Task<IActionResult> GetAll(
        [FromQuery] FlightSearchQuery query,
        CancellationToken cancellationToken)
    {
        var flights = await _flightService.SearchAsync(
            query, cancellationToken);

        return Ok(flights);
    }

    [ProducesResponseType(typeof(FlightResponse), StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status404NotFound)]
    [HttpGet("{id:int}")]
    public async Task<IActionResult> GetById(
        [FromRoute] int id,
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
    [ProducesResponseType(typeof(IReadOnlyList<string>), StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status401Unauthorized)]
    [ProducesResponseType(StatusCodes.Status404NotFound)]
    [HttpGet("{id:int}/reserved-seats")]
    public async Task<IActionResult> GetReservedSeats(
        [FromRoute] int id,
        CancellationToken cancellationToken)
    {
        var seats = await _flightService.GetReservedSeatsAsync(
            id, cancellationToken);

        if (seats is null)
            return NotFound();

        return Ok(seats);
    }

    [Authorize(Roles = Roles.Admin)]
    [ProducesResponseType(typeof(FlightResponse), StatusCodes.Status201Created)]
    [ProducesResponseType(typeof(ValidationProblemDetails), StatusCodes.Status400BadRequest)]
    [ProducesResponseType(StatusCodes.Status401Unauthorized)]
    [ProducesResponseType(StatusCodes.Status403Forbidden)]
    [ProducesResponseType(StatusCodes.Status404NotFound)]
    [ProducesResponseType(typeof(ProblemDetails), StatusCodes.Status409Conflict)]
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

    [Authorize(Roles = Roles.Admin)]
    [ProducesResponseType(typeof(FlightResponse), StatusCodes.Status200OK)]
    [ProducesResponseType(typeof(ValidationProblemDetails), StatusCodes.Status400BadRequest)]
    [ProducesResponseType(StatusCodes.Status401Unauthorized)]
    [ProducesResponseType(StatusCodes.Status403Forbidden)]
    [ProducesResponseType(StatusCodes.Status404NotFound)]
    [ProducesResponseType(typeof(ProblemDetails), StatusCodes.Status409Conflict)]
    [HttpPut("{id:int}")]
    public async Task<IActionResult> Update(
        [FromRoute] int id,
        [FromBody] FlightRequest request,
        CancellationToken cancellationToken)
    {
        var flight = await _flightService.UpdateAsync(
            id, request, cancellationToken);

        if (flight is null)
            return NotFound();

        return Ok(flight);
    }

    [Authorize(Roles = Roles.Admin)]
    [ProducesResponseType(StatusCodes.Status204NoContent)]
    [ProducesResponseType(StatusCodes.Status401Unauthorized)]
    [ProducesResponseType(StatusCodes.Status403Forbidden)]
    [ProducesResponseType(StatusCodes.Status404NotFound)]
    [ProducesResponseType(typeof(ProblemDetails), StatusCodes.Status409Conflict)]
    [HttpDelete("{id:int}")]
    public async Task<IActionResult> Delete(
        [FromRoute] int id,
        CancellationToken cancellationToken)
    {
        var deleted = await _flightService.DeleteAsync(
            id, cancellationToken);

        return deleted ? NoContent() : NotFound();
    }
}
