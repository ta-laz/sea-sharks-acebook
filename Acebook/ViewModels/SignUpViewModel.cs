using System.ComponentModel.DataAnnotations;
namespace Acebook.ViewModels;

public class SignUpViewModel : IValidatableObject
{


    [Required(ErrorMessage = "First Name is required")]
    public string FirstName { get; set; } = "";

    [Required(ErrorMessage = "Last Name is required")]
    public string LastName { get; set; } = "";

    [Required(ErrorMessage = "Date of birth is required")]
    public DateOnly DOB { get; set; }

    [Required(ErrorMessage = "Email is required")]
    [EmailAddress(ErrorMessage = "Enter a valid email address")]
    public string Email { get; set; } = "";

    [Required(ErrorMessage = "Password is required")]
    [DataType(DataType.Password)]
    [RegularExpression(@"^(?=.*[A-Z])(?=.*[^A-Za-z0-9]).{8,}$",
        ErrorMessage = "Password must be ≥ 8 chars and include an uppercase letter and a special character.")]
    public string Password { get; set; } = "";

    [Required(ErrorMessage = "Confirmation is required")]
    [DataType(DataType.Password)]
    [Compare(nameof(Password), ErrorMessage = "Passwords do not match.")]
    public string ConfirmPassword { get; set; } = "";

    public IEnumerable<ValidationResult> Validate(ValidationContext validationContext)
        {
            if (DOB == default) yield break; // Required handles empty

            DateOnly today = DateOnly.FromDateTime(DateTime.Today);
            int age = today.Year - DOB.Year;
            if (DOB > today.AddYears(-age)) age--; // adjust if birthday not reached

            if (age < 12)
            {
                yield return new ValidationResult(
                    "You must be at least 12 years old.",
                    new[] { nameof(DOB) }  // attach the error to the DOB field
                );
            }
        }
}

