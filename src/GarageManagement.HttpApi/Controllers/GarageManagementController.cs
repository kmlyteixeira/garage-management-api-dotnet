using GarageManagement.Localization;
using Volo.Abp.AspNetCore.Mvc;

namespace GarageManagement.Controllers;

/* Inherit your controllers from this class.
 */
public abstract class GarageManagementController : AbpControllerBase
{
    protected GarageManagementController()
    {
        LocalizationResource = typeof(GarageManagementResource);
    }
}
