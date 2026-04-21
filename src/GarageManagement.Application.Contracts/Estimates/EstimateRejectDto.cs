using System.ComponentModel.DataAnnotations;

namespace GarageManagement.Estimates;

public class EstimateRejectDto
{
    [Required]
    [StringLength(500)]
    public string Reason { get; set; } = string.Empty;
}
