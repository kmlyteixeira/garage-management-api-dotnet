using System;
using System.Threading.Tasks;

namespace GarageManagement.ServiceOrders;

public class ServiceOrderUpdateMediator : IServiceOrderUpdateMediator
{
    private readonly IServiceOrderUpdateHandler handler;

    public ServiceOrderUpdateMediator(IServiceOrderUpdateHandler handler)
    {
        this.handler = handler;
    }

    public Task<ServiceOrderDto> UpdateAsync(ServiceOrder serviceOrder, ServiceOrderUpdateDto input)
    {
        return handler.HandleAsync(serviceOrder, input);
    }
}