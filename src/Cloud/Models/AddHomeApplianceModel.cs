using System.ComponentModel.DataAnnotations;

namespace IntelliHome.Cloud;

public sealed class AddHomeApplianceModel
{
    [Required]
    public Guid Id { get; set; }
}

