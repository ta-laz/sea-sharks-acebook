using System.ComponentModel.DataAnnotations;
namespace Acebook.ViewModels;

public class SignInViewModel : IValidatableObject
{
    [Required(ErrorMessage = "Email is required")]
    [EmailAddress(ErrorMessage = "Enter a valid email address")]
    public string Email { get; set; } = "";

    [Required(ErrorMessage = "Password is required")]
    [DataType(DataType.Password)]
    //[RegularExpression(@"^(?=.*[A-Z])(?=.*[^A-Za-z0-9]).{8,}$",
        //ErrorMessage = "Password must be ≥ 8 chars and include an uppercase letter and a special character.")]
    public string Password { get; set; } = "";

    public IEnumerable<ValidationResult> Validate(ValidationContext validationContext)
    {
        yield break;
    }
}

