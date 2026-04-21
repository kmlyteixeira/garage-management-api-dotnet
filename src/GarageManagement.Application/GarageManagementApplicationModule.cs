using Volo.Abp.Account;
using Volo.Abp.AutoMapper;
using Volo.Abp.FeatureManagement;
using Volo.Abp.Identity;
using Volo.Abp.Modularity;
using Volo.Abp.PermissionManagement;
using Volo.Abp.SettingManagement;
using Volo.Abp.TenantManagement;
using Microsoft.Extensions.DependencyInjection;
using GarageManagement.ServiceOrders;

namespace GarageManagement;

[DependsOn(
    typeof(GarageManagementDomainModule),
    typeof(AbpAccountApplicationModule),
    typeof(AbpAutoMapperModule),
    typeof(GarageManagementApplicationContractsModule),
    typeof(AbpIdentityApplicationModule),
    typeof(AbpPermissionManagementApplicationModule),
    typeof(AbpTenantManagementApplicationModule),
    typeof(AbpFeatureManagementApplicationModule),
    typeof(AbpSettingManagementApplicationModule)
    )]
public class GarageManagementApplicationModule : AbpModule
{
    public override void ConfigureServices(ServiceConfigurationContext context)
    {
        context.Services.AddAutoMapperObjectMapper<GarageManagementApplicationModule>();

        context.Services.AddTransient<IServiceOrderUpdateHandler, ServiceOrderUpdateHandler>();
        context.Services.AddTransient<IServiceOrderUpdateMediator, ServiceOrderUpdateMediator>();

        Configure<AbpAutoMapperOptions>(options => {
            options.AddMaps<GarageManagementApplicationModule>();
        });
    }
}
