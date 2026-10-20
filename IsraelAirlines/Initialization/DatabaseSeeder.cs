using IsraelAirlines.core.Constants;
using IsraelAirlines.core.Entities;
using IsraelAirlines.data.Contex;
using Microsoft.AspNetCore.Identity;
using Microsoft.EntityFrameworkCore;

namespace IsraelAirlines.api.Initialization;

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

        var adminPassword = configuration["Seed:AdminPassword"];

        if (string.IsNullOrWhiteSpace(adminPassword))
        {
            if (!environment.IsDevelopment())
            {
                logger.LogWarning(
                    "Seed:AdminPassword is not set; skipping user seed.");
                return;
            }

            adminPassword = "Admin123!";
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
        if (environment.IsDevelopment())
        {
            var regularUser = new User
            {
                FullName = "Test User",
                Email = "user@example.com",
                Role = Roles.User
            };

            regularUser.PasswordHash = passwordHasher.HashPassword(
                regularUser, "User123!");

            context.Users.Add(regularUser);
        }

        await context.SaveChangesAsync();
    }
}
