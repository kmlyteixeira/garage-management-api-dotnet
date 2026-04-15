using Microsoft.Extensions.Localization;
using GarageManagement.Localization;
using Volo.Abp.DependencyInjection;
using Volo.Abp.Ui.Branding;

namespace GarageManagement;

[Dependency(ReplaceServices = true)]
public class GarageManagementBrandingProvider : DefaultBrandingProvider
{
    private IStringLocalizer<GarageManagementResource> _localizer;

    public GarageManagementBrandingProvider(IStringLocalizer<GarageManagementResource> localizer)
    {
        _localizer = localizer;
    }

    public override string AppName => _localizer["AppName"];
}
