
using IsraelAirlines.core.Constants;
using IsraelAirlines.core.DTOs.Requests;
using IsraelAirlines.core.DTOs.Responses;
using IsraelAirlines.core.Interfaces.Services;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace IsraelAirlines.API.Controllers;

// "aircraft" is both the singular and the plural form.
[ApiController]
[Route("api/aircraft")]
public class AircraftController : ControllerBase
{
    private readonly IAircraftService _aircraftService;

    public AircraftController(IAircraftService aircraftService)
    {
        _aircraftService = aircraftService;
    }

    [ProducesResponseType(typeof(IReadOnlyList<AircraftResponse>), StatusCodes.Status200OK)]
    [HttpGet]
    public async Task<IActionResult> GetAll(
        CancellationToken cancellationToken)
    {
        var aircraft = await _aircraftService.GetAllAsync(
            cancellationToken);

        return Ok(aircraft);
    }

    [ProducesResponseType(typeof(AircraftResponse), StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status404NotFound)]
    [HttpGet("{id:int}")]
    public async Task<IActionResult> GetById(
        [FromRoute] int id,
        CancellationToken cancellationToken)
    {
        var aircraft = await _aircraftService.GetByIdAsync(
            id, cancellationToken);

        if (aircraft is null)
            return NotFound();

        return Ok(aircraft);
    }

    [Authorize(Roles = Roles.Admin)]
    [ProducesResponseType(typeof(AircraftResponse), StatusCodes.Status201Created)]
    [ProducesResponseType(typeof(ValidationProblemDetails), StatusCodes.Status400BadRequest)]
    [ProducesResponseType(StatusCodes.Status401Unauthorized)]
    [ProducesResponseType(StatusCodes.Status403Forbidden)]
    [ProducesResponseType(typeof(ProblemDetails), StatusCodes.Status409Conflict)]
    [HttpPost]
    public async Task<IActionResult> Create(
        [FromBody] AircraftRequest request,
        CancellationToken cancellationToken)
    {
        var aircraft = await _aircraftService.CreateAsync(
            request, cancellationToken);

        return CreatedAtAction(
            nameof(GetById),
            new { id = aircraft.Id },
            aircraft);
    }

    [Authorize(Roles = Roles.Admin)]
    [ProducesResponseType(typeof(AircraftResponse), StatusCodes.Status200OK)]
    [ProducesResponseType(typeof(ValidationProblemDetails), StatusCodes.Status400BadRequest)]
    [ProducesResponseType(StatusCodes.Status401Unauthorized)]
    [ProducesResponseType(StatusCodes.Status403Forbidden)]
    [ProducesResponseType(StatusCodes.Status404NotFound)]
    [ProducesResponseType(typeof(ProblemDetails), StatusCodes.Status409Conflict)]
    [HttpPut("{id:int}")]
    public async Task<IActionResult> Update(
        [FromRoute] int id,
        [FromBody] AircraftRequest request,
        CancellationToken cancellationToken)
    {
        var aircraft = await _aircraftService.UpdateAsync(
            id, request, cancellationToken);

        if (aircraft is null)
            return NotFound();

        return Ok(aircraft);
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
        var deleted = await _aircraftService.DeleteAsync(
            id, cancellationToken);

        return deleted ? NoContent() : NotFound();
    }
}
