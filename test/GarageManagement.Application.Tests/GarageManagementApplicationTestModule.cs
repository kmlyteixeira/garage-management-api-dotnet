using Volo.Abp.Modularity;

namespace GarageManagement;

[DependsOn(
    typeof(GarageManagementApplicationModule),
    typeof(GarageManagementDomainTestModule)
)]
public class GarageManagementApplicationTestModule : AbpModule
{

}
