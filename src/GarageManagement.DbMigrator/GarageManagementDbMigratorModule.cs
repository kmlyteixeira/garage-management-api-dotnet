using GarageManagement.EntityFrameworkCore;
using Volo.Abp.Autofac;
using Volo.Abp.Modularity;

namespace GarageManagement.DbMigrator;

[DependsOn(
    typeof(AbpAutofacModule),
    typeof(GarageManagementEntityFrameworkCoreModule),
    typeof(GarageManagementApplicationContractsModule)
    )]
public class GarageManagementDbMigratorModule : AbpModule
{
}
