
using AutoMapper;
using IsraelAirlines.core.DTOs.Requests;
using IsraelAirlines.core.DTOs.Responses;
using IsraelAirlines.core.Entities;
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
        var aircraft = new Aircraft
        {
            Model = request.Model.Trim(),
            RegistrationNumber = request.RegistrationNumber.Trim(),
            IsActive = request.IsActive
        };

        await _aircraftRepository.AddAsync(
            aircraft, cancellationToken);

        return _mapper.Map<AircraftResponse>(aircraft);
    }
}
