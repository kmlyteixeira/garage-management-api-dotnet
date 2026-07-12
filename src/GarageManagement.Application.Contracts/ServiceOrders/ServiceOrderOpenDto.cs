using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;

namespace GarageManagement.ServiceOrders;

public class ServiceOrderOpenDto
{
    [Required]
    [StringLength(50)]
    public string ServiceOrderNumber { get; set; } = string.Empty;

    [Required]
    public Guid CustomerId { get; set; }

    [Required]
    public Guid VehicleId { get; set; }

    public List<ServiceOrderServiceItemCreateDto> ServiceItems { get; set; } = new();

    public List<ServiceOrderProductItemCreateDto> PartItems { get; set; } = new();
}
