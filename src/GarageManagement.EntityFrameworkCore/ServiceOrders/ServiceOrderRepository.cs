using System;
using System.Threading.Tasks;
using GarageManagement.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore;
using Volo.Abp.Domain.Repositories.EntityFrameworkCore;
using Volo.Abp.EntityFrameworkCore;

namespace GarageManagement.ServiceOrders;

public class ServiceOrderRepository : EfCoreRepository<GarageManagementDbContext, ServiceOrder, Guid>,
    IServiceOrderRepository
{
    public ServiceOrderRepository(IDbContextProvider<GarageManagementDbContext> dbContextProvider)
        : base(dbContextProvider)
    {
    }

    public async Task<ServiceOrder> GetWithDetailsAsync(Guid id)
    {
        var query = await GetDbSetAsync();

        return await query
            .Include(s => s.Estimate)
                .ThenInclude(e => e.ServiceItems)
            .FirstOrDefaultAsync(s => s.Id == id)
            ?? throw new Exception("Service order not found.");
    }
}