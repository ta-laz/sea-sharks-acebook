using System.ComponentModel.DataAnnotations;

namespace acebook.ViewModels;

public class SignInViewModel : IValidatableObject
{
    [Required(ErrorMessage = "email is required")]
    [DataType(DataType.EmailAddress)]
    public string Email { get; set; } = "";

    [Required(ErrorMessage = "password is required")]
    [DataType(DataType.Password)]
    [RegularExpression(@"^(?=.*[A-Z])(?=.*[^A-Za-z0-9]).{8,}$",
        ErrorMessage = "Password must be ≥ 8 chars and include an uppercase letter and a special character.")]
    public string Password { get; set; } = "";
}

