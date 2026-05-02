using System;
using System.Threading.Tasks;
using GarageManagement.Inventories;
using GarageManagement.Products;
using Shouldly;
using Volo.Abp;
using Volo.Abp.Domain.Repositories;
using Xunit;

namespace GarageManagement.EntityFrameworkCore.Applications;

[Collection(GarageManagementTestConsts.CollectionDefinitionName)]
public class InventoryAppServiceIntegrationTests : GarageManagementEntityFrameworkCoreTestBase
{
    private readonly InventoryAppService inventoryAppService;
    private readonly IRepository<Product, Guid> productRepository;
    private readonly IRepository<Inventory, Guid> inventoryRepository;

    public InventoryAppServiceIntegrationTests()
    {
        inventoryAppService = GetRequiredService<InventoryAppService>();
        productRepository = GetRequiredService<IRepository<Product, Guid>>();
        inventoryRepository = GetRequiredService<IRepository<Inventory, Guid>>();
    }

    [Fact]
    public async Task IsProductAvailableAsync_Should_Handle_Zero_And_Positive_Quantities()
    {
        var product = new Product("Filtro", 35m);

        await WithUnitOfWorkAsync(async () =>
        {
            await productRepository.InsertAsync(product, autoSave: true);
            await inventoryRepository.InsertAsync(new Inventory(product, 10), autoSave: true);

            (await inventoryAppService.IsProductAvailableAsync(product.Id, 0)).ShouldBeTrue();
            (await inventoryAppService.IsProductAvailableAsync(product.Id, 5)).ShouldBeTrue();
            (await inventoryAppService.IsProductAvailableAsync(product.Id, 11)).ShouldBeFalse();
        });
    }

    [Fact]
    public async Task Reserve_Release_And_Consume_Should_Update_Inventory()
    {
        var product = new Product("Pastilha", 90m);

        await WithUnitOfWorkAsync(async () =>
        {
            await productRepository.InsertAsync(product, autoSave: true);
            var inventory = new Inventory(product, 10);
            await inventoryRepository.InsertAsync(inventory, autoSave: true);

            await inventoryAppService.ReserveStockAsync(product.Id, 4);
            await inventoryAppService.ReleaseReservedStockAsync(product.Id, 1);
            await inventoryAppService.ConsumeReservedStockAsync(product.Id, 2);

            var updated = await inventoryRepository.GetAsync(inventory.Id);
            updated.Quantity.ShouldBe(8);
            updated.ReservedQuantity.ShouldBe(1);
        });
    }

    [Fact]
    public async Task DecreaseStockAsync_Should_Update_Quantity_And_Reject_Invalid_Requests()
    {
        var product = new Product("Bateria", 400m);

        await WithUnitOfWorkAsync(async () =>
        {
            await productRepository.InsertAsync(product, autoSave: true);
            var inventory = new Inventory(product, 10);
            await inventoryRepository.InsertAsync(inventory, autoSave: true);

            await inventoryAppService.DecreaseStockAsync(product.Id, 3);

            var updated = await inventoryRepository.GetAsync(inventory.Id);
            updated.Quantity.ShouldBe(7);

            await inventoryAppService.DecreaseStockAsync(product.Id, 0);

            await Should.ThrowAsync<UserFriendlyException>(() => inventoryAppService.DecreaseStockAsync(Guid.NewGuid(), 1));
        });
    }

    [Fact]
    public async Task Zero_Quantities_Should_Be_NoOps()
    {
        var product = new Product("Oleo", 50m);

        await WithUnitOfWorkAsync(async () =>
        {
            await productRepository.InsertAsync(product, autoSave: true);
            var inventory = new Inventory(product, 10);
            await inventoryRepository.InsertAsync(inventory, autoSave: true);

            await inventoryAppService.ReserveStockAsync(product.Id, 0);
            await inventoryAppService.ReleaseReservedStockAsync(product.Id, 0);
            await inventoryAppService.ConsumeReservedStockAsync(product.Id, 0);

            var updated = await inventoryRepository.GetAsync(inventory.Id);
            updated.Quantity.ShouldBe(10);
            updated.ReservedQuantity.ShouldBe(0);
        });
    }
}