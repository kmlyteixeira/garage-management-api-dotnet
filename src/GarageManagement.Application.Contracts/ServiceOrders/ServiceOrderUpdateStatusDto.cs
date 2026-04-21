using System.ComponentModel.DataAnnotations;

namespace GarageManagement.ServiceOrders;

public class ServiceOrderUpdateStatusDto
{
    [EnumDataType(typeof(ServiceOrderStatus))]
    public ServiceOrderStatus? Status { get; set; } = ServiceOrderStatus.InDiagnosis;
}
