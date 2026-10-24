using System.ComponentModel.DataAnnotations;

namespace BookStore.Api.Contracts;

public static class Validation
{
    /// Returns a 400 ValidationProblem result when invalid, otherwise null.
    public static IResult? Check<T>(T model) where T : notnull
    {
        var results = new List<ValidationResult>();
        if (Validator.TryValidateObject(model, new ValidationContext(model), results, validateAllProperties: true))
            return null;

        var errors = results
            .GroupBy(r => r.MemberNames.FirstOrDefault() ?? "request")
            .ToDictionary(g => g.Key, g => g.Select(r => r.ErrorMessage ?? "Invalid value").ToArray());
        return Results.ValidationProblem(errors);
    }
}