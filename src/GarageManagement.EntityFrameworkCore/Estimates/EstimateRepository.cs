using System;
using System.Threading.Tasks;
using GarageManagement.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore;
using Volo.Abp.Domain.Repositories.EntityFrameworkCore;
using Volo.Abp.EntityFrameworkCore;

namespace GarageManagement.Estimates;

public class EstimateRepository : EfCoreRepository<GarageManagementDbContext, Estimate, Guid>,
    IEstimateRepository
{
    public EstimateRepository(IDbContextProvider<GarageManagementDbContext> dbContextProvider)
        : base(dbContextProvider)
    {
    }

    public async Task<Estimate> GetWithDetailsAsync(Guid id)
    {
        var query = await GetDbSetAsync();

        return await query
            .Include(e => e.ServiceItems)
                .ThenInclude(s => s.Service)
            .Include(e => e.PartItems)
                .ThenInclude(p => p.Product)
            .FirstOrDefaultAsync(s => s.Id == id)
            ?? throw new Exception("Estimate not found.");
    }
}