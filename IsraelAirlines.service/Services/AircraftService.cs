
using AutoMapper;
using IsraelAirlines.core.DTOs.Requests;
using IsraelAirlines.core.DTOs.Responses;
using IsraelAirlines.core.Entities;
using IsraelAirlines.core.Exceptions;
using IsraelAirlines.core.Interfaces.Repository;
using IsraelAirlines.core.Interfaces.Services;

namespace IsraelAirlines.Service.Services;

public class AircraftService : IAircraftService
{
    private readonly IAircraftRepository _aircraftRepository;
    private readonly IMapper _mapper;

    public AircraftService(
        IAircraftRepository aircraftRepository,
        IMapper mapper)
    {
        _aircraftRepository = aircraftRepository;
        _mapper = mapper;
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

        var aircraft = new Aircraft
        {
            Model = request.Model.Trim(),
            RegistrationNumber = registrationNumber,
            IsActive = request.IsActive,
            Rows = request.Rows,
            SeatLayout = seatLayout,
            BusinessRows = request.BusinessRows
        };

        await _aircraftRepository.AddAsync(
            aircraft, cancellationToken);

        return _mapper.Map<AircraftResponse>(aircraft);
    }
}
