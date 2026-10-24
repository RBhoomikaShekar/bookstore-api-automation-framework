using BookStore.Api.Auth;
using BookStore.Api.Contracts;
using BookStore.Api.Data;
using BookStore.Api.Domain;
using Microsoft.AspNetCore.Identity;
using Microsoft.EntityFrameworkCore;

namespace BookStore.Api.Endpoints;

public static class AuthEndpoints
{
    public static void MapAuthEndpoints(this IEndpointRouteBuilder app)
    {
        var g = app.MapGroup("/auth").WithTags("Auth");

        g.MapPost("/register", async (RegisterRequest req, AppDbContext db, IPasswordHasher<AppUser> hasher) =>
        {
            if (Validation.Check(req) is { } bad) return bad;
            if (await db.Users.AnyAsync(u => u.Username == req.Username))
                return Results.Problem(statusCode: 409, title: "Username already exists");

            var user = new AppUser { Username = req.Username!, PasswordHash = "", Role = "User" };
            user.PasswordHash = hasher.HashPassword(user, req.Password!);
            db.Users.Add(user);
            await db.SaveChangesAsync();
            return Results.Created($"/users/{user.Id}", new { user.Id, user.Username, user.Role });
        });

        g.MapPost("/login", async (LoginRequest req, AppDbContext db,
                                   IPasswordHasher<AppUser> hasher, TokenService tokens) =>
        {
            if (Validation.Check(req) is { } bad) return bad;
            var user = await db.Users.FirstOrDefaultAsync(u => u.Username == req.Username);
            if (user is null ||
                hasher.VerifyHashedPassword(user, user.PasswordHash, req.Password!) == PasswordVerificationResult.Failed)
                return Results.Problem(statusCode: 401, title: "Invalid credentials");

            return Results.Ok(tokens.Create(user));
        });
    }
}