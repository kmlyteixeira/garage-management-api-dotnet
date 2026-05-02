using System;
using System.Reflection;
using System.Threading.Tasks;
using GarageManagement.Vehicles;
using Shouldly;
using Volo.Abp;
using Volo.Abp.Domain.Repositories;
using Xunit;

namespace GarageManagement.EntityFrameworkCore.Applications;

[Collection(GarageManagementTestConsts.CollectionDefinitionName)]
public class VehicleAppServiceIntegrationTests : GarageManagementEntityFrameworkCoreTestBase
{
    private readonly VehicleAppService vehicleAppService;

    public VehicleAppServiceIntegrationTests()
    {
        vehicleAppService = GetRequiredService<VehicleAppService>();
    }

    [Fact]
    public async Task CreateAsync_Should_Persist_Vehicle()
    {
        var result = await WithUnitOfWorkAsync(() => vehicleAppService.CreateAsync(new VehicleCreateUpdateDto
        {
            Make = "Ford",
            Model = "Ka",
            Year = 2020,
            LicensePlate = "ABC-1234"
        }));

        result.Make.ShouldBe("Ford");
        result.Model.ShouldBe("Ka");
        result.Year.ShouldBe(2020);
    }

    [Fact]
    public async Task UpdateAsync_Should_Persist_Changes()
    {
        await WithUnitOfWorkAsync(async () =>
        {
            var created = await vehicleAppService.CreateAsync(new VehicleCreateUpdateDto
            {
                Make = "Fiat",
                Model = "Argo",
                Year = 2021,
                LicensePlate = "DEF-2345"
            });

            var updated = await vehicleAppService.UpdateAsync(created.Id, new VehicleCreateUpdateDto
            {
                Make = "Fiat",
                Model = "Pulse",
                Year = 2022,
                LicensePlate = "DEF-2345"
            });

            updated.Model.ShouldBe("Pulse");
            updated.Year.ShouldBe(2022);
        });
    }

    [Fact]
    public async Task CreateAsync_Should_Reject_Duplicate_License_Plate()
    {
        await WithUnitOfWorkAsync(async () =>
        {
            await vehicleAppService.CreateAsync(new VehicleCreateUpdateDto
            {
                Make = "Toyota",
                Model = "Corolla",
                Year = 2020,
                LicensePlate = "GHI-3456"
            });

            await Should.ThrowAsync<UserFriendlyException>(() => vehicleAppService.CreateAsync(new VehicleCreateUpdateDto
            {
                Make = "Honda",
                Model = "Civic",
                Year = 2021,
                LicensePlate = "GHI-3456"
            }));
        });
    }

    [Fact]
    public void ContainsConstraintName_Should_Detect_In_Nested_Exceptions()
    {
        var method = typeof(VehicleAppService).GetMethod("ContainsConstraintName", BindingFlags.NonPublic | BindingFlags.Static);
        method.ShouldNotBeNull();

        var nestedException = new Exception("outer", new Exception("duplicate key value violates unique constraint \"IX_AppVehicles_LicensePlate\""));
        var result = (bool)method!.Invoke(null, new object[] { nestedException, "IX_AppVehicles_LicensePlate" })!;

        result.ShouldBeTrue();
    }
}