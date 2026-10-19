
using IsraelAirlines.core.DTOs.Requests;
using IsraelAirlines.core.Interfaces;
using IsraelAirlines.core.Interfaces.Services;
using Microsoft.AspNetCore.Mvc;

namespace IsraelAirlines.API.Controllers;

[ApiController]
[Route("api/[controller]")]
public class AircraftController : ControllerBase
{
    private readonly IAircraftService _aircraftService;

    public AircraftController(IAircraftService aircraftService)
    {
        _aircraftService = aircraftService;
    }
    [ProducesResponseType(StatusCodes.Status200OK)]
    [HttpGet]
    public async Task<IActionResult> GetAll(
        CancellationToken cancellationToken)
    {
        var aircraft = await _aircraftService.GetAllAsync(
            cancellationToken);

        return Ok(aircraft);
    }
    [ProducesResponseType(StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status404NotFound)]
    [HttpGet("{id:int}")]
    public async Task<IActionResult> GetById(
        int id,
        CancellationToken cancellationToken)
    {
        var aircraft = await _aircraftService.GetByIdAsync(
            id, cancellationToken);

        if (aircraft is null)
            return NotFound();

        return Ok(aircraft);
    }
    [ProducesResponseType(StatusCodes.Status201Created)]
    [ProducesResponseType(StatusCodes.Status400BadRequest)]
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
}
