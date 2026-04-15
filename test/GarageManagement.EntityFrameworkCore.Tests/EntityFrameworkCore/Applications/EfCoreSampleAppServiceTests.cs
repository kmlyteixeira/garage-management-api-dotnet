using GarageManagement.Samples;
using Xunit;

namespace GarageManagement.EntityFrameworkCore.Applications;

[Collection(GarageManagementTestConsts.CollectionDefinitionName)]
public class EfCoreSampleAppServiceTests : SampleAppServiceTests<GarageManagementEntityFrameworkCoreTestModule>
{

}
