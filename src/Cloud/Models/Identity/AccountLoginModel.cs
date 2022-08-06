using System.ComponentModel.DataAnnotations;

namespace IntelliHome.Cloud.Identity;

public sealed class AccountLoginModel
{
    [Required]
    [EmailAddress(ErrorMessage = "Invalid Email")]
    public string? Email { get; set; }

    [Required]
    public string? Password { get; set; }
}