
using AutoMapper;
using IsraelAirlines.core.DTOs.Requests;
using IsraelAirlines.core.DTOs.Responses;
using IsraelAirlines.core.Entities;
using IsraelAirlines.core.Exceptions;
using IsraelAirlines.core.Interfaces.Repository;
using IsraelAirlines.core.Interfaces.Services;
using Microsoft.Extensions.Logging;

namespace IsraelAirlines.Service.Services;

public class AircraftService : IAircraftService
{
    private readonly IAircraftRepository _aircraftRepository;
    private readonly IUnitOfWork _unitOfWork;
    private readonly IMapper _mapper;
    private readonly ILogger<AircraftService> _logger;

    public AircraftService(
        IAircraftRepository aircraftRepository,
        IUnitOfWork unitOfWork,
        IMapper mapper,
        ILogger<AircraftService> logger)
    {
        _aircraftRepository = aircraftRepository;
        _unitOfWork = unitOfWork;
        _mapper = mapper;
        _logger = logger;
    }

    public async Task<AircraftResponse?> GetByIdAsync(
        int id,
        CancellationToken cancellationToken = default)
    {
        var aircraft = await _aircraftRepository.GetByIdAsync(
            id, cancellationToken);

        if (aircraft is null)
            return null;

        return _mapper.Map<AircraftResponse>(aircraft);
    }

    public async Task<IReadOnlyList<AircraftResponse>> GetAllAsync(
        CancellationToken cancellationToken = default)
    {
        var aircraftList = await _aircraftRepository.GetAllAsync(
            cancellationToken);

        return _mapper.Map<IReadOnlyList<AircraftResponse>>(
            aircraftList);
    }

    public async Task<AircraftResponse> CreateAsync(
        AircraftRequest request,
        CancellationToken cancellationToken = default)
    {
        var registrationNumber = request.RegistrationNumber.Trim();

        var existingAircraft = await _aircraftRepository.GetByRegistrationNumberAsync(
            registrationNumber, cancellationToken);

        if (existingAircraft is not null)
        {
            throw new ConflictException(
                "An aircraft with this registration number already exists.");
        }

        var aircraft = _mapper.Map<Aircraft>(request);
        aircraft.SeatLayout = NormalizeSeatLayout(request);

        await _aircraftRepository.AddAsync(
            aircraft, cancellationToken);

        _logger.LogInformation(
            "Aircraft {RegistrationNumber} created", aircraft.RegistrationNumber);

        return _mapper.Map<AircraftResponse>(aircraft);
    }

    public async Task<AircraftResponse?> UpdateAsync(
        int id,
        AircraftRequest request,
        CancellationToken cancellationToken = default)
    {
        var aircraft = await _aircraftRepository.GetForUpdateAsync(
            id, cancellationToken);

        if (aircraft is null)
            return null;

        var registrationNumber = request.RegistrationNumber.Trim();

        if (registrationNumber != aircraft.RegistrationNumber)
        {
            var existingAircraft = await _aircraftRepository.GetByRegistrationNumberAsync(
                registrationNumber, cancellationToken);

            if (existingAircraft is not null)
            {
                throw new ConflictException(
                    "An aircraft with this registration number already exists.");
            }
        }

        var seatLayout = NormalizeSeatLayout(request);
        var layoutChanged = seatLayout != aircraft.SeatLayout || request.Rows != aircraft.Rows;

        // Existing flights already have seats built from the current layout.
        if (layoutChanged &&
            await _aircraftRepository.HasFlightsAsync(aircraft.Id, cancellationToken))
        {
            throw new ConflictException(
                "The seat layout of an aircraft with flights cannot be changed.");
        }

        _mapper.Map(request, aircraft);
        aircraft.SeatLayout = seatLayout;

        await _unitOfWork.SaveChangesAsync(cancellationToken);

        _logger.LogInformation("Aircraft {AircraftId} updated", aircraft.Id);

        return _mapper.Map<AircraftResponse>(aircraft);
    }

    public async Task<bool> DeleteAsync(
        int id,
        CancellationToken cancellationToken = default)
    {
        var aircraft = await _aircraftRepository.GetForUpdateAsync(
            id, cancellationToken);

        if (aircraft is null)
            return false;

        if (await _aircraftRepository.HasFlightsAsync(aircraft.Id, cancellationToken))
        {
            throw new ConflictException(
                "An aircraft that has flights cannot be deleted.");
        }

        _aircraftRepository.Remove(aircraft);
        await _unitOfWork.SaveChangesAsync(cancellationToken);

        _logger.LogInformation("Aircraft {AircraftId} deleted", id);

        return true;
    }

    // Upper-case letters with single spaces for aisles, e.g. "ABC DEF".
    private static string NormalizeSeatLayout(AircraftRequest request)
    {
        var seatLayout = string.Join(' ',
            request.SeatLayout.Trim().ToUpperInvariant()
                .Split(' ', StringSplitOptions.RemoveEmptyEntries));
        var seatLetters = seatLayout.Replace(" ", string.Empty);

        if (seatLetters.Distinct().Count() != seatLetters.Length)
        {
            throw new BadRequestException(
                "Seat layout must not repeat a seat letter.");
        }

        if (request.BusinessRows > request.Rows)
        {
            throw new BadRequestException(
                "Business rows cannot exceed the total number of rows.");
        }

        return seatLayout;
    }
}
