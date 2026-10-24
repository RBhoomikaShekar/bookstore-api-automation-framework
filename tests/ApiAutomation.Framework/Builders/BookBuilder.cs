using Bogus;
using ApiAutomation.Framework.Models;

namespace ApiAutomation.Framework.Builders;

public class BookBuilder
{
    private string? _title, _author, _isbn;
    private decimal _price;

    public BookBuilder()
    {
        var f = new Faker();                                   // Bogus: fake data generator
        _title = f.Lorem.Sentence(3).TrimEnd('.');
        _author = f.Name.FullName();
        _isbn = "978-" + f.Random.Replace("##########");     // # = random digit
        _price = Math.Round(f.Random.Decimal(5, 100), 2);
    }

    public BookBuilder WithTitle(string? value) { _title = value; return this; }
    public BookBuilder WithAuthor(string? value) { _author = value; return this; }
    public BookBuilder WithIsbn(string? value) { _isbn = value; return this; }
    public BookBuilder WithPrice(decimal value) { _price = value; return this; }

    public BookRequest Build() => new() { Title = _title, Author = _author, Isbn = _isbn, Price = _price };
}