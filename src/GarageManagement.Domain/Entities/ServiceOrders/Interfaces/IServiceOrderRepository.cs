using System;
using System.Threading.Tasks;
using Volo.Abp.Domain.Repositories;

namespace GarageManagement.ServiceOrders;

public interface IServiceOrderRepository : IRepository<ServiceOrder, Guid>
{
    Task<ServiceOrder> GetWithDetailsAsync(Guid id);
}