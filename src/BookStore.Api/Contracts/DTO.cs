using System.ComponentModel.DataAnnotations;
using BookStore.Api.Domain;

namespace BookStore.Api.Contracts;

public class BookRequest
{
    [Required, StringLength(200)] public string? Title { get; set; }
    [Required, StringLength(100)] public string? Author { get; set; }
    [Required, StringLength(20, MinimumLength = 5)] public string? Isbn { get; set; }
    [Range(0.01, 1_000_000)] public decimal Price { get; set; }
}

public class RegisterRequest
{
    [Required, StringLength(50, MinimumLength = 3)] public string? Username { get; set; }
    [Required, StringLength(100, MinimumLength = 6)] public string? Password { get; set; }
}

public class LoginRequest
{
    [Required] public string? Username { get; set; }
    [Required] public string? Password { get; set; }
}

public record TokenResponse(string AccessToken, DateTime ExpiresAtUtc);
public record PagedResult<T>(List<T> Items, int Page, int PageSize, int TotalCount);

public record BookResponse(int Id, string Title, string Author, string Isbn, decimal Price, DateTime CreatedAtUtc)
{
    public static BookResponse From(Book b) => new(b.Id, b.Title, b.Author, b.Isbn, b.Price, b.CreatedAtUtc);
}