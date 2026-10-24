namespace ApiAutomation.Framework.Models;

// What we SEND when creating or updating a book
public class BookRequest
{
    public string? Title { get; set; }
    public string? Author { get; set; }
    public string? Isbn { get; set; }
    public decimal Price { get; set; }
}

// What we RECEIVE back
public record BookDto(int Id, string Title, string Author, string Isbn, decimal Price, DateTime CreatedAtUtc);
public record PagedResult<T>(List<T> Items, int Page, int PageSize, int TotalCount);
public record TokenDto(string AccessToken, DateTime ExpiresAtUtc);
public record ProblemDetailsDto(string? Title, int? Status, string? Detail, Dictionary<string, string[]>? Errors);