using System.Collections.Generic;

namespace GarageManagement.ServiceOrders;

public class ServiceOrderUpdateDto
{
    public List<ServiceOrderServiceItemCreateDto> ServiceItems { get; set; } = new();
    public List<ServiceOrderProductItemCreateDto> PartItems { get; set; } = new();
}
