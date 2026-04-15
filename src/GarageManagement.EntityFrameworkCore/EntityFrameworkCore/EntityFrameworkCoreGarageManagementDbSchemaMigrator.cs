using System;
using System.Threading.Tasks;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.DependencyInjection;
using GarageManagement.Data;
using Volo.Abp.DependencyInjection;

namespace GarageManagement.EntityFrameworkCore;

public class EntityFrameworkCoreGarageManagementDbSchemaMigrator
    : IGarageManagementDbSchemaMigrator, ITransientDependency
{
    private readonly IServiceProvider _serviceProvider;

    public EntityFrameworkCoreGarageManagementDbSchemaMigrator(
        IServiceProvider serviceProvider)
    {
        _serviceProvider = serviceProvider;
    }

    public async Task MigrateAsync()
    {
        /* We intentionally resolve the GarageManagementDbContext
         * from IServiceProvider (instead of directly injecting it)
         * to properly get the connection string of the current tenant in the
         * current scope.
         */

        await _serviceProvider
            .GetRequiredService<GarageManagementDbContext>()
            .Database
            .MigrateAsync();
    }
}
