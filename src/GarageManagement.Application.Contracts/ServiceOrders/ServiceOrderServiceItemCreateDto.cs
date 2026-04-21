using System;
using System.ComponentModel.DataAnnotations;

namespace GarageManagement.ServiceOrders;

public class ServiceOrderServiceItemCreateDto
{
    [Required]
    public Guid ServiceId { get; set; }

    [Range(1, int.MaxValue)]
    public int Quantity { get; set; }
}
