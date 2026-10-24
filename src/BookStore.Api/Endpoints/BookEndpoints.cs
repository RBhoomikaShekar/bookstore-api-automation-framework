using BookStore.Api.Contracts;
using BookStore.Api.Data;
using BookStore.Api.Domain;
using Microsoft.EntityFrameworkCore;

namespace BookStore.Api.Endpoints;

public static class BookEndpoints
{
    public static void MapBookEndpoints(this IEndpointRouteBuilder app)
    {
        var g = app.MapGroup("/books").WithTags("Books");

        g.MapGet("/", async (AppDbContext db, int page = 1, int pageSize = 10,
                             string? search = null, string? sortBy = null, bool desc = false) =>
        {
            if (page < 1 || pageSize is < 1 or > 100)
                return Results.ValidationProblem(new Dictionary<string, string[]>
                { ["paging"] = ["page must be >= 1 and pageSize must be between 1 and 100"] });

            var q = db.Books.AsNoTracking();
            if (!string.IsNullOrWhiteSpace(search))
                q = q.Where(b => b.Title.Contains(search) || b.Author.Contains(search));

            IQueryable<Book> ordered = (sortBy?.ToLowerInvariant(), desc) switch
            {
                ("price", false) => q.OrderBy(b => b.Price),
                ("price", true) => q.OrderByDescending(b => b.Price),
                ("title", false) => q.OrderBy(b => b.Title),
                ("title", true) => q.OrderByDescending(b => b.Title),
                (_, true) => q.OrderByDescending(b => b.Id),
                _ => q.OrderBy(b => b.Id)
            };

            var total = await ordered.CountAsync();
            var rows = await ordered.Skip((page - 1) * pageSize).Take(pageSize).ToListAsync();
            return Results.Ok(new PagedResult<BookResponse>(rows.Select(BookResponse.From).ToList(), page, pageSize, total));
        });

        g.MapGet("/{id:int}", async (int id, AppDbContext db) =>
            await db.Books.AsNoTracking().FirstOrDefaultAsync(b => b.Id == id) is { } b
                ? Results.Ok(BookResponse.From(b))
                : Results.NotFound());

        g.MapPost("/", async (BookRequest req, AppDbContext db) =>
        {
            if (Validation.Check(req) is { } bad) return bad;
            if (await db.Books.AnyAsync(b => b.Isbn == req.Isbn))
                return Results.Problem(statusCode: 409, title: "A book with this ISBN already exists");

            var book = new Book { Title = req.Title!, Author = req.Author!, Isbn = req.Isbn!, Price = req.Price };
            db.Books.Add(book);
            try { await db.SaveChangesAsync(); }
            catch (DbUpdateException)   // race: another request inserted same ISBN between check and save
            { return Results.Problem(statusCode: 409, title: "A book with this ISBN already exists"); }

            return Results.Created($"/books/{book.Id}", BookResponse.From(book));
        }).RequireAuthorization();

        g.MapPut("/{id:int}", async (int id, BookRequest req, AppDbContext db) =>
        {
            if (Validation.Check(req) is { } bad) return bad;
            var book = await db.Books.FindAsync(id);
            if (book is null) return Results.NotFound();
            if (await db.Books.AnyAsync(b => b.Isbn == req.Isbn && b.Id != id))
                return Results.Problem(statusCode: 409, title: "A book with this ISBN already exists");

            book.Title = req.Title!; book.Author = req.Author!; book.Isbn = req.Isbn!; book.Price = req.Price;
            await db.SaveChangesAsync();
            return Results.Ok(BookResponse.From(book));
        }).RequireAuthorization();

        g.MapDelete("/{id:int}", async (int id, AppDbContext db) =>
        {
            var book = await db.Books.FindAsync(id);
            if (book is null) return Results.NotFound();
            db.Books.Remove(book);
            await db.SaveChangesAsync();
            return Results.NoContent();
        }).RequireAuthorization("AdminOnly");
    }
}