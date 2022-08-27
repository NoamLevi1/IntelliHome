using System.ComponentModel.DataAnnotations;

namespace IntelliHome.Cloud;

public sealed class AccountForgotPasswordModel
{
    [Required]
    [EmailAddress(ErrorMessage = "Invalid Email")]
    public string? Email { get; set; }
}