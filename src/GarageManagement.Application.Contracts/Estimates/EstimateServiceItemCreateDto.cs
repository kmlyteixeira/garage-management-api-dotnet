using System;
using System.ComponentModel.DataAnnotations;

namespace GarageManagement.Estimates;

public class EstimateServiceItemCreateDto
{
    [Required]
    public Guid ServiceId { get; set; }

    [Range(1, int.MaxValue)]
    public int Quantity { get; set; }
}
