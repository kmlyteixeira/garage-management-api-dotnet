using GarageManagement.Localization;
using Volo.Abp.Authorization.Permissions;
using Volo.Abp.Localization;

namespace GarageManagement.Permissions;

public class GarageManagementPermissionDefinitionProvider : PermissionDefinitionProvider
{
    public override void Define(IPermissionDefinitionContext context)
    {
        var myGroup = context.AddGroup(GarageManagementPermissions.GroupName);
        //Define your own permissions here. Example:
        //myGroup.AddPermission(GarageManagementPermissions.MyPermission1, L("Permission:MyPermission1"));
    }

    private static LocalizableString L(string name)
    {
        return LocalizableString.Create<GarageManagementResource>(name);
    }
}
