using System;
using System.Threading.Tasks;
using GarageManagement.Customers;
using GarageManagement.Shared;
using Shouldly;
using Xunit;

namespace GarageManagement.EntityFrameworkCore.Applications;

[Collection(GarageManagementTestConsts.CollectionDefinitionName)]
public class CustomerAppServiceIntegrationTests : GarageManagementEntityFrameworkCoreTestBase
{
    private readonly CustomerAppService customerAppService;

    public CustomerAppServiceIntegrationTests()
    {
        customerAppService = GetRequiredService<CustomerAppService>();
    }

    [Fact]
    public async Task CreateAsync_Should_Persist_Customer()
    {
        var suffix = Guid.NewGuid().ToString("N")[..8];

        var result = await WithUnitOfWorkAsync(() => customerAppService.CreateAsync(new CustomerCreateUpdateDto
        {
            Name = $"Cliente {suffix}",
            Email = $"cliente-{suffix}@mail.com",
            PhoneNumber = "11999999999",
            Document = "12345678901"
        }));

        result.Name.ShouldStartWith("Cliente ");
        result.Email.ShouldContain("@mail.com");
        result.Document.ShouldBe("12345678901");
    }

    [Fact]
    public async Task UpdateAsync_Should_Persist_Changes()
    {
        var suffix = Guid.NewGuid().ToString("N")[..8];

        await WithUnitOfWorkAsync(async () =>
        {
            var created = await customerAppService.CreateAsync(new CustomerCreateUpdateDto
            {
                Name = $"Cliente {suffix}",
                Email = $"cliente-{suffix}@mail.com",
                PhoneNumber = "11999999999",
                Document = "12345678901"
            });

            var updated = await customerAppService.UpdateAsync(created.Id, new CustomerCreateUpdateDto
            {
                Name = $"Cliente Alterado {suffix}",
                Email = $"cliente-alterado-{suffix}@mail.com",
                PhoneNumber = "11888888888",
                Document = "12345678901"
            });

            updated.Name.ShouldStartWith("Cliente Alterado");
            updated.PhoneNumber.ShouldBe("11888888888");
        });
    }

    [Fact]
    public void ContainsConstraintName_Should_Detect_In_Nested_Exceptions()
    {
        var constraintName = "IX_AppCustomers_Document";
        var nestedException = new Exception("outer", new Exception($"duplicate key value violates unique constraint \"{constraintName}\""));

        DbConstraintExceptionHelper.ContainsConstraintName(nestedException, constraintName).ShouldBeTrue();
    }

    [Fact]
    public void ContainsConstraintName_Should_Return_False_When_Missing()
    {
        DbConstraintExceptionHelper.ContainsConstraintName(new Exception("no match"), "IX_AppCustomers_Document").ShouldBeFalse();
    }
}