using System;
using System.Threading.Tasks;
using Volo.Abp.Domain.Repositories;

namespace GarageManagement.Estimates;

public interface IEstimateRepository : IRepository<Estimate, Guid>
{
    Task<Estimate> GetWithDetailsAsync(Guid id);
}