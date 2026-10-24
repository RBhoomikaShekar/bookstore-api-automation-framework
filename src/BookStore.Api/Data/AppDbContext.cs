using BookStore.Api.Domain;
using Microsoft.EntityFrameworkCore;

namespace BookStore.Api.Data;

public class AppDbContext(DbContextOptions<AppDbContext> options) : DbContext(options)
{
    public DbSet<Book> Books => Set<Book>();
    public DbSet<AppUser> Users => Set<AppUser>();

    protected override void OnModelCreating(ModelBuilder b)
    {
        b.Entity<Book>().HasIndex(x => x.Isbn).IsUnique();       // DB-level uniqueness
        // SQLite stores decimal as TEXT and cannot ORDER BY it; store as double instead.
        b.Entity<Book>().Property(x => x.Price).HasConversion<double>();
        b.Entity<AppUser>().HasIndex(x => x.Username).IsUnique();
    }
}