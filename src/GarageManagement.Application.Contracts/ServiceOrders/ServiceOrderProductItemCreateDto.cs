using System;
using System.ComponentModel.DataAnnotations;

namespace GarageManagement.ServiceOrders;

public class ServiceOrderProductItemCreateDto
{
    [Required]
    public Guid ProductId { get; set; }

    [Range(1, int.MaxValue)]
    public int Quantity { get; set; }
}
