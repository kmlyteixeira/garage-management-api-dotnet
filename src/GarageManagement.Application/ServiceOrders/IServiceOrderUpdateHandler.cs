using System;
using System.Threading.Tasks;

namespace GarageManagement.ServiceOrders;

public interface IServiceOrderUpdateHandler
{
    Task<ServiceOrderDto> HandleAsync(ServiceOrder serviceOrder, ServiceOrderUpdateDto input);
}