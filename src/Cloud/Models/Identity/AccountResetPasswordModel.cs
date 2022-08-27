using System.ComponentModel.DataAnnotations;

namespace IntelliHome.Cloud;

public sealed class AccountResetPasswordModel
{
    [Required]
    [DataType(DataType.Password)]
    public string? Password { get; set; }
    [DataType(DataType.Password)]
    [Compare(nameof(Password), ErrorMessage = "The password and confirmation password do not match.")]
    public string? ConfirmPassword { get; set; }
    public string? Email { get; set; }
    public string? Token { get; set; }
}