using GarageManagement.Localization;
using Volo.Abp.Authorization.Permissions;
using Volo.Abp.Localization;

namespace GarageManagement.Permissions;

public class GarageManagementPermissionDefinitionProvider : PermissionDefinitionProvider
{
    public override void Define(IPermissionDefinitionContext context)
    {
        var group = context.AddGroup(GarageManagementPermissions.GroupName, L("Permission:GarageManagement"));

        var customers = group.AddPermission(GarageManagementPermissions.Customers.Default, L("Permission:Customers"));
        customers.AddChild(GarageManagementPermissions.Customers.Create, L("Permission:Customers.Create"));
        customers.AddChild(GarageManagementPermissions.Customers.Edit, L("Permission:Customers.Edit"));
        customers.AddChild(GarageManagementPermissions.Customers.Delete, L("Permission:Customers.Delete"));

        var vehicles = group.AddPermission(GarageManagementPermissions.Vehicles.Default, L("Permission:Vehicles"));
        vehicles.AddChild(GarageManagementPermissions.Vehicles.Create, L("Permission:Vehicles.Create"));
        vehicles.AddChild(GarageManagementPermissions.Vehicles.Edit, L("Permission:Vehicles.Edit"));
        vehicles.AddChild(GarageManagementPermissions.Vehicles.Delete, L("Permission:Vehicles.Delete"));

        var products = group.AddPermission(GarageManagementPermissions.Products.Default, L("Permission:Products"));
        products.AddChild(GarageManagementPermissions.Products.Create, L("Permission:Products.Create"));
        products.AddChild(GarageManagementPermissions.Products.Edit, L("Permission:Products.Edit"));
        products.AddChild(GarageManagementPermissions.Products.Delete, L("Permission:Products.Delete"));

        var services = group.AddPermission(GarageManagementPermissions.Services.Default, L("Permission:Services"));
        services.AddChild(GarageManagementPermissions.Services.Create, L("Permission:Services.Create"));
        services.AddChild(GarageManagementPermissions.Services.Edit, L("Permission:Services.Edit"));
        services.AddChild(GarageManagementPermissions.Services.Delete, L("Permission:Services.Delete"));

        var inventories = group.AddPermission(GarageManagementPermissions.Inventories.Default, L("Permission:Inventories"));
        inventories.AddChild(GarageManagementPermissions.Inventories.Create, L("Permission:Inventories.Create"));
        inventories.AddChild(GarageManagementPermissions.Inventories.Edit, L("Permission:Inventories.Edit"));
        inventories.AddChild(GarageManagementPermissions.Inventories.Delete, L("Permission:Inventories.Delete"));

        var estimates = group.AddPermission(GarageManagementPermissions.Estimates.Default, L("Permission:Estimates"));
        estimates.AddChild(GarageManagementPermissions.Estimates.Create, L("Permission:Estimates.Create"));
        estimates.AddChild(GarageManagementPermissions.Estimates.Edit, L("Permission:Estimates.Edit"));
        estimates.AddChild(GarageManagementPermissions.Estimates.Delete, L("Permission:Estimates.Delete"));
        estimates.AddChild(GarageManagementPermissions.Estimates.Approve, L("Permission:Estimates.Approve"));
        estimates.AddChild(GarageManagementPermissions.Estimates.Reject, L("Permission:Estimates.Reject"));

        var serviceOrders = group.AddPermission(GarageManagementPermissions.ServiceOrders.Default, L("Permission:ServiceOrders"));
        serviceOrders.AddChild(GarageManagementPermissions.ServiceOrders.Create, L("Permission:ServiceOrders.Create"));
        serviceOrders.AddChild(GarageManagementPermissions.ServiceOrders.Edit, L("Permission:ServiceOrders.Edit"));
        serviceOrders.AddChild(GarageManagementPermissions.ServiceOrders.Delete, L("Permission:ServiceOrders.Delete"));
        serviceOrders.AddChild(GarageManagementPermissions.ServiceOrders.UpdateStatus, L("Permission:ServiceOrders.UpdateStatus"));
    }

    private static LocalizableString L(string name)
    {
        return LocalizableString.Create<GarageManagementResource>(name);
    }
}
