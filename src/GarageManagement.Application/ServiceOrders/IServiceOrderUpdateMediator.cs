using System;
using System.Threading.Tasks;

namespace GarageManagement.ServiceOrders;

public interface IServiceOrderUpdateMediator
{
    Task<ServiceOrderDto> UpdateAsync(ServiceOrder serviceOrder, ServiceOrderUpdateDto input);
}