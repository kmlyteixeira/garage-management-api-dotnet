using System;
using System.Collections.Generic;
using System.Text;
using GarageManagement.Localization;
using Volo.Abp.Application.Services;

namespace GarageManagement;

/* Inherit your application services from this class.
 */
public abstract class GarageManagementAppService : ApplicationService
{
    protected GarageManagementAppService()
    {
        LocalizationResource = typeof(GarageManagementResource);
    }
}
