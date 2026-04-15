using Xunit;

namespace GarageManagement.EntityFrameworkCore;

[CollectionDefinition(GarageManagementTestConsts.CollectionDefinitionName)]
public class GarageManagementEntityFrameworkCoreCollection : ICollectionFixture<GarageManagementEntityFrameworkCoreFixture>
{

}
