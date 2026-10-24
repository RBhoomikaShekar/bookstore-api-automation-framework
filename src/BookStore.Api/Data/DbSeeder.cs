using BookStore.Api.Domain;
using Microsoft.AspNetCore.Identity;

namespace BookStore.Api.Data;

public static class DbSeeder
{
    public static async Task SeedAsync(WebApplication app)
    {
        using var scope = app.Services.CreateScope();
        var db = scope.ServiceProvider.GetRequiredService<AppDbContext>();
        var hasher = scope.ServiceProvider.GetRequiredService<IPasswordHasher<AppUser>>();

        await db.Database.EnsureCreatedAsync();   // creates the SQLite file + tables if missing

        var username = app.Configuration["Admin:Username"] ?? "admin";
        var password = app.Configuration["Admin:Password"];
        if (string.IsNullOrWhiteSpace(password))
            throw new InvalidOperationException("Admin:Password is not configured.");

        if (!db.Users.Any(u => u.Username == username))
        {
            var admin = new AppUser { Username = username, PasswordHash = "", Role = "Admin" };
            admin.PasswordHash = hasher.HashPassword(admin, password);
            db.Users.Add(admin);
            await db.SaveChangesAsync();
        }
    }
}