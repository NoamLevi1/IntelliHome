using System.ComponentModel.DataAnnotations;

namespace IntelliHome.Cloud;

public sealed class EditHomeApplianceModel
{
    public Guid Id { get; set; }
    [Required]
    public string? Name { get; set; }
}