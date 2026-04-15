using Volo.Abp.Modularity;

namespace GarageManagement;

[DependsOn(
    typeof(GarageManagementDomainModule),
    typeof(GarageManagementTestBaseModule)
)]
public class GarageManagementDomainTestModule : AbpModule
{

}
