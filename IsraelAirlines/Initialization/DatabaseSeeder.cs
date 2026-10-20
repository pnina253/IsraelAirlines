using IsraelAirlines.core.Constants;
using IsraelAirlines.core.Entities;
using IsraelAirlines.data.Context;
using Microsoft.AspNetCore.Identity;
using Microsoft.EntityFrameworkCore;

namespace IsraelAirlines.API.Initialization;

public static class DatabaseSeeder
{
    public static async Task SeedAsync(IServiceProvider services)
    {
        using var scope = services.CreateScope();

        var context = scope.ServiceProvider
            .GetRequiredService<DataContext>();

        var passwordHasher = scope.ServiceProvider
            .GetRequiredService<IPasswordHasher<User>>();

        var configuration = scope.ServiceProvider
            .GetRequiredService<IConfiguration>();

        var environment = scope.ServiceProvider
            .GetRequiredService<IHostEnvironment>();

        var logger = scope.ServiceProvider
            .GetRequiredService<ILoggerFactory>()
            .CreateLogger("DatabaseSeeder");

        if (await context.Users.AnyAsync())
            return;

        // Demo passwords come from configuration (appsettings.Development.json or User Secrets),
        // never from code.
        var adminPassword = configuration["Seed:AdminPassword"];

        if (string.IsNullOrWhiteSpace(adminPassword))
        {
            logger.LogWarning(
                "Seed:AdminPassword is not set; skipping user seed.");
            return;
        }

        var admin = new User
        {
            FullName = "Admin User",
            Email = "admin@example.com",
            Role = Roles.Admin
        };

        admin.PasswordHash = passwordHasher.HashPassword(
            admin, adminPassword);

        context.Users.Add(admin);

        // The test user with a known password exists only in Development.
        var userPassword = configuration["Seed:UserPassword"];

        if (environment.IsDevelopment() && !string.IsNullOrWhiteSpace(userPassword))
        {
            var regularUser = new User
            {
                FullName = "Test User",
                Email = "user@example.com",
                Role = Roles.User
            };

            regularUser.PasswordHash = passwordHasher.HashPassword(
                regularUser, userPassword);

            context.Users.Add(regularUser);
        }

        await context.SaveChangesAsync();

        logger.LogInformation("Seeded the demo users");
    }
}
