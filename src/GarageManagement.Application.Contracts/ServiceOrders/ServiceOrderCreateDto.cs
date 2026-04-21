using System;
using System.ComponentModel.DataAnnotations;

namespace GarageManagement.ServiceOrders;

public class ServiceOrderCreateDto
{
    [Required]
    [StringLength(50)]
    public string ServiceOrderNumber { get; set; } = string.Empty;
    public Guid? EstimateId { get; set; }
    public Guid CustomerId { get; set; }
    public Guid VehicleId { get; set; }
}
