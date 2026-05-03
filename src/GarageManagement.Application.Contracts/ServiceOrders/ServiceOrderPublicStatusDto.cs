using System;

namespace GarageManagement.ServiceOrders;

public class ServiceOrderPublicStatusDto
{
    public string ServiceOrderNumber { get; set; } = string.Empty;
    public ServiceOrderStatus Status { get; set; }
    public DateTime CreatedAt { get; set; }
}