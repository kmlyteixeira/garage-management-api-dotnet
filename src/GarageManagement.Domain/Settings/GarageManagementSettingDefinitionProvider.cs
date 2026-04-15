using Volo.Abp.Settings;

namespace GarageManagement.Settings;

public class GarageManagementSettingDefinitionProvider : SettingDefinitionProvider
{
    public override void Define(ISettingDefinitionContext context)
    {
        //Define your own settings here. Example:
        //context.Add(new SettingDefinition(GarageManagementSettings.MySetting1));
    }
}
