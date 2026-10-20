using IsraelAirlines.data.Context;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Configuration;
using Xunit;

namespace IsraelAirlines.Tests.Integration;

// Integration tests need a real, migrated PostgreSQL database. The connection string comes from
// this project's User Secrets, or from the ConnectionStrings__DefaultConnection environment variable.
// Run only the unit tests with: dotnet test --filter "Category!=Integration"
public static class TestDatabase
{
    public static DbContextOptions<DataContext> Options()
    {
        var configuration = new ConfigurationBuilder()
            .AddUserSecrets<DatabaseConnectionTests>()
            .AddEnvironmentVariables()
            .Build();

        var connectionString =
            configuration.GetConnectionString("DefaultConnection");

        Assert.False(
            string.IsNullOrWhiteSpace(connectionString),
            "Connection string was not found.");

        return new DbContextOptionsBuilder<DataContext>()
            .UseNpgsql(connectionString)
            .Options;
    }
}

[Trait("Category", "Integration")]
public class DatabaseConnectionTests
{
    [Fact]
    public async Task CanConnectToPostgreSql()
    {
        await using var context = new DataContext(TestDatabase.Options());

        var canConnect = await context.Database.CanConnectAsync();

        Assert.True(
            canConnect,
            "Could not connect to PostgreSQL.");
    }
}
