using Volo.Abp.Modularity;

namespace GarageManagement;

public abstract class GarageManagementApplicationTestBase<TStartupModule> : GarageManagementTestBase<TStartupModule>
    where TStartupModule : IAbpModule
{

}
