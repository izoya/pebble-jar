using System.ComponentModel.DataAnnotations;

namespace PebbleJar.Api.Contracts.Requests;

public sealed class UpdateAccountRequest : IValidatableObject
{
    [StringLength(200)]
    [RegularExpression(@".*\S.*")]
    public string? Name { get; init; }

    public bool? IsSyncEnabled { get; init; }

    public IEnumerable<ValidationResult> Validate(ValidationContext validationContext)
    {
        if (Name is null && IsSyncEnabled is null)
        {
            yield return new ValidationResult(
                "Provide at least one account setting to update.",
                [nameof(Name), nameof(IsSyncEnabled)]);
        }
    }
}
