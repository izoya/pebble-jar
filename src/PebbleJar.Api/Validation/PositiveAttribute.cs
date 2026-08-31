using System.ComponentModel.DataAnnotations;

namespace PebbleJar.Api.Validation;

public sealed class PositiveAttribute : ValidationAttribute
{
    public override bool IsValid(object? value) => value is decimal v && v > 0;
}
