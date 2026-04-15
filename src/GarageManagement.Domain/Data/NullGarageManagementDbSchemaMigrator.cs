using System.Threading.Tasks;
using Volo.Abp.DependencyInjection;

namespace GarageManagement.Data;

/* This is used if database provider does't define
 * IGarageManagementDbSchemaMigrator implementation.
 */
public class NullGarageManagementDbSchemaMigrator : IGarageManagementDbSchemaMigrator, ITransientDependency
{
    public Task MigrateAsync()
    {
        return Task.CompletedTask;
    }
}
