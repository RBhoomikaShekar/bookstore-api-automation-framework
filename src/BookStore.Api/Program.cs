using System.Text;
using BookStore.Api.Auth;
using BookStore.Api.Data;
using BookStore.Api.Domain;
using BookStore.Api.Endpoints;
using Microsoft.AspNetCore.Authentication.JwtBearer;
using Microsoft.AspNetCore.Identity;
using Microsoft.IdentityModel.Tokens;
using Microsoft.EntityFrameworkCore;

SQLitePCL.Batteries_V2.Init();   // registers the native SQLite provider

var builder = WebApplication.CreateBuilder(args);

var jwt = builder.Configuration.GetSection("Jwt").Get<JwtOptions>()
          ?? throw new InvalidOperationException("Jwt section missing.");
if (jwt.Key.Length < 32)
    throw new InvalidOperationException("Jwt:Key must be at least 32 characters.");

builder.Services.Configure<JwtOptions>(builder.Configuration.GetSection("Jwt"));
builder.Services.AddDbContext<AppDbContext>(o =>
    o.UseSqlite(builder.Configuration.GetConnectionString("Default")));
builder.Services.AddSingleton<TokenService>();
builder.Services.AddSingleton<IPasswordHasher<AppUser>, PasswordHasher<AppUser>>();
builder.Services.AddProblemDetails();
builder.Services.AddOpenApi();

builder.Services.AddAuthentication(JwtBearerDefaults.AuthenticationScheme).AddJwtBearer(o =>
{
    o.TokenValidationParameters = new TokenValidationParameters
    {
        ValidateIssuer = true,
        ValidIssuer = jwt.Issuer,
        ValidateAudience = true,
        ValidAudience = jwt.Audience,
        ValidateLifetime = true,
        ValidateIssuerSigningKey = true,
        IssuerSigningKey = new SymmetricSecurityKey(Encoding.UTF8.GetBytes(jwt.Key)),
        ClockSkew = TimeSpan.Zero,           // default is 5 min; zero makes expiry tests deterministic
        NameClaimType = "name",
        RoleClaimType = "role"
    };
});
builder.Services.AddAuthorizationBuilder()
    .AddPolicy("AdminOnly", p => p.RequireRole("Admin"));

var app = builder.Build();

await DbSeeder.SeedAsync(app);

app.UseExceptionHandler();      // unhandled exceptions -> 500 ProblemDetails (no stack trace leaks)
app.UseStatusCodePages();       // empty 401/404/405 -> ProblemDetails body
app.UseAuthentication();        // who are you?
app.UseAuthorization();         // are you allowed?

app.MapOpenApi();app.MapGet("/health", () => Results.Ok(new { status = "Healthy" })).WithTags("Health");
app.MapAuthEndpoints();
app.MapBookEndpoints();

app.Run();

public partial class Program;   // lets WebApplicationFactory reference the app later (stretch goal)