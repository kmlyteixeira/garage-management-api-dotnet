using Volo.Abp.Modularity;

namespace GarageManagement;

/* Inherit from this class for your domain layer tests. */
public abstract class GarageManagementDomainTestBase<TStartupModule> : GarageManagementTestBase<TStartupModule>
    where TStartupModule : IAbpModule
{

}
