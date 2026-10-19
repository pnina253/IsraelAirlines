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

        if (await context.Users.AnyAsync())
            return;

        var admin = new User
        {
            Id = 1,
            FullName = "Admin User",
            Email = "admin@example.com",
            Role = "Admin"
        };

        admin.PasswordHash = passwordHasher.HashPassword(
            admin, "Admin123!");

        var regularUser = new User
        {
            Id = 2,
            FullName = "Test User",
            Email = "user@example.com",
            Role = "User"
        };

        regularUser.PasswordHash = passwordHasher.HashPassword(
            regularUser, "User123!");

        context.Users.AddRange(admin, regularUser);
        await context.SaveChangesAsync();
    }
}