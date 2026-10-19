
using AutoMapper;
using IsraelAirlines.core.Entities;
using IsraelAirlines.core.DTOs.Responses;

namespace IsraelAirlines.service.Services.Mapping;

public class MappingProfile : Profile
{
    public MappingProfile()
    {
        CreateMap<Aircraft, AircraftResponse>();

        CreateMap<Flight, FlightResponse>()
            .ForMember(
                dest => dest.AircraftModel,
                opt => opt.MapFrom(src => src.Aircraft.Model))
            .ForMember(
                dest => dest.RegistrationNumber,
                opt => opt.MapFrom(src => src.Aircraft.RegistrationNumber));

        CreateMap<User, UserResponse>();

        CreateMap<Reservation, ReservationResponse>()
            .ForMember(
                dest => dest.FlightNumber,
                opt => opt.MapFrom(src => src.Flight.FlightNumber));
    }
}
