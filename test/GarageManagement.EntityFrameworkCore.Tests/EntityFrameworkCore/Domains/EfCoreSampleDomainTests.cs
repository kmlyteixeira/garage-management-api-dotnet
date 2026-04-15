using GarageManagement.Samples;
using Xunit;

namespace GarageManagement.EntityFrameworkCore.Domains;

[Collection(GarageManagementTestConsts.CollectionDefinitionName)]
public class EfCoreSampleDomainTests : SampleDomainTests<GarageManagementEntityFrameworkCoreTestModule>
{

}
