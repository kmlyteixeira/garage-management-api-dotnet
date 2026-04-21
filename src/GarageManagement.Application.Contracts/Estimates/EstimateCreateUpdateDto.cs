using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;

namespace GarageManagement.Estimates;

public class EstimateCreateUpdateDto
{
    [Required]
    [StringLength(50)]
    public string EstimateNumber { get; set; } = string.Empty;

    [Required]
    public Guid CustomerId { get; set; }

    [Required]
    public Guid VehicleId { get; set; }

    public List<EstimateServiceItemCreateDto> ServiceItems { get; set; } = new();
    public List<EstimateProductItemCreateDto> PartItems { get; set; } = new();
}
