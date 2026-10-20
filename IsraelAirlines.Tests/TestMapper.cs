using AutoMapper;
using IsraelAirlines.Service.Services.Mapping;
using Microsoft.Extensions.Logging.Abstractions;

namespace IsraelAirlines.Tests;

// The real AutoMapper profile, for tests that check what the mapping produces.
public static class TestMapper
{
    public static MapperConfiguration Configuration { get; } = new(
        cfg => cfg.AddProfile<MappingProfile>(),
        NullLoggerFactory.Instance);

    public static IMapper Create() => Configuration.CreateMapper();
}

public class MappingProfileTests
{
    [Fact]
    public void MappingProfile_IsValid()
    {
        TestMapper.Configuration.AssertConfigurationIsValid();
    }
}
