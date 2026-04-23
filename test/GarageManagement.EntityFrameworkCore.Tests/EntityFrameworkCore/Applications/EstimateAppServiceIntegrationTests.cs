using System;
using System.Threading.Tasks;
using GarageManagement.Customers;
using GarageManagement.Estimates;
using GarageManagement.Inventories;
using GarageManagement.Products;
using GarageManagement.ServiceOrders;
using GarageManagement.Vehicles;
using Shouldly;
using Volo.Abp;
using Volo.Abp.Domain.Entities;
using Volo.Abp.Domain.Repositories;
using Xunit;

namespace GarageManagement.EntityFrameworkCore.Applications;

[Collection(GarageManagementTestConsts.CollectionDefinitionName)]
public class EstimateAppServiceIntegrationTests : GarageManagementEntityFrameworkCoreTestBase
{
    private readonly EstimateAppService estimateAppService;
    private readonly IRepository<Customer, Guid> customerRepository;
    private readonly IRepository<Vehicle, Guid> vehicleRepository;
    private readonly IRepository<Product, Guid> productRepository;
    private readonly IRepository<Inventory, Guid> inventoryRepository;
    private readonly IRepository<Estimate, Guid> estimateRepository;
    private readonly IRepository<ServiceOrder, Guid> serviceOrderRepository;

    public EstimateAppServiceIntegrationTests()
    {
        estimateAppService = GetRequiredService<EstimateAppService>();
        customerRepository = GetRequiredService<IRepository<Customer, Guid>>();
        vehicleRepository = GetRequiredService<IRepository<Vehicle, Guid>>();
        productRepository = GetRequiredService<IRepository<Product, Guid>>();
        inventoryRepository = GetRequiredService<IRepository<Inventory, Guid>>();
        estimateRepository = GetRequiredService<IRepository<Estimate, Guid>>();
        serviceOrderRepository = GetRequiredService<IRepository<ServiceOrder, Guid>>();
    }

    [Fact]
    public async Task ApproveAsync_Should_Consume_Reserved_Stock_And_Move_ServiceOrder_To_WaitingExecution()
    {
        var suffix = Guid.NewGuid().ToString("N")[..8];
        var customer = new Customer($"Cliente {suffix}", $"{suffix}@mail.com", "11999999999", new Document($"DOC{suffix}"));
        var vehicle = new Vehicle("Ford", "Ka", 2020, $"ABC{suffix}");
        var product = new Product($"Produto {suffix}", 50m);

        await WithUnitOfWorkAsync(async () =>
        {
            await customerRepository.InsertAsync(customer, autoSave: true);
            await vehicleRepository.InsertAsync(vehicle, autoSave: true);
            await productRepository.InsertAsync(product, autoSave: true);

            var inventory = new Inventory(product, 10);
            inventory.ReserveStock(2);
            await inventoryRepository.InsertAsync(inventory, autoSave: true);

            var estimate = new Estimate(Guid.NewGuid(), $"EST-{suffix}", customer.Id, vehicle.Id);
            estimate.AddPartItem(product, 2);
            estimate.SendToCustomer();
            await estimateRepository.InsertAsync(estimate, autoSave: true);

            var serviceOrder = new ServiceOrder(Guid.NewGuid(), $"OS-{suffix}", customer.Id, vehicle.Id);
            serviceOrder.AssociateEstimate(estimate.Id);
            serviceOrder.WaitApproval();
            await serviceOrderRepository.InsertAsync(serviceOrder, autoSave: true);

            await estimateAppService.ApproveAsync(estimate.Id);

            var updatedEstimate = await estimateRepository.GetAsync(estimate.Id);
            var updatedServiceOrder = await serviceOrderRepository.GetAsync(serviceOrder.Id);
            var updatedInventory = await inventoryRepository.GetAsync(inventory.Id);

            updatedEstimate.Status.ShouldBe(EstimateStatus.Approved);
            updatedServiceOrder.Status.ShouldBe(ServiceOrderStatus.WaitingExecution);
            updatedInventory.Quantity.ShouldBe(8);
            updatedInventory.ReservedQuantity.ShouldBe(0);
        });
    }

    [Fact]
    public async Task RejectAsync_Should_Release_Reserved_Stock_And_Cancel_ServiceOrder()
    {
        var suffix = Guid.NewGuid().ToString("N")[..8];
        var customer = new Customer($"Cliente {suffix}", $"{suffix}@mail.com", "11999999999", new Document($"DOC{suffix}"));
        var vehicle = new Vehicle("GM", "Onix", 2021, $"DEF{suffix}");
        var product = new Product($"Produto {suffix}", 75m);

        await WithUnitOfWorkAsync(async () =>
        {
            await customerRepository.InsertAsync(customer, autoSave: true);
            await vehicleRepository.InsertAsync(vehicle, autoSave: true);
            await productRepository.InsertAsync(product, autoSave: true);

            var inventory = new Inventory(product, 12);
            inventory.ReserveStock(3);
            await inventoryRepository.InsertAsync(inventory, autoSave: true);

            var estimate = new Estimate(Guid.NewGuid(), $"EST-{suffix}", customer.Id, vehicle.Id);
            estimate.AddPartItem(product, 3);
            estimate.SendToCustomer();
            await estimateRepository.InsertAsync(estimate, autoSave: true);

            var serviceOrder = new ServiceOrder(Guid.NewGuid(), $"OS-{suffix}", customer.Id, vehicle.Id);
            serviceOrder.AssociateEstimate(estimate.Id);
            serviceOrder.WaitApproval();
            await serviceOrderRepository.InsertAsync(serviceOrder, autoSave: true);

            await estimateAppService.RejectAsync(estimate.Id, new EstimateRejectDto { Reason = "Cliente recusou" });

            var updatedEstimate = await estimateRepository.GetAsync(estimate.Id);
            var updatedServiceOrder = await serviceOrderRepository.GetAsync(serviceOrder.Id);
            var updatedInventory = await inventoryRepository.GetAsync(inventory.Id);

            updatedEstimate.Status.ShouldBe(EstimateStatus.Rejected);
            updatedEstimate.RejectionReason.ShouldBe("Cliente recusou");
            updatedServiceOrder.Status.ShouldBe(ServiceOrderStatus.Canceled);
            updatedInventory.Quantity.ShouldBe(12);
            updatedInventory.ReservedQuantity.ShouldBe(0);
        });
    }

    [Fact]
    public async Task ApproveAsync_Should_Throw_UserFriendlyException_When_Estimate_Does_Not_Exist()
    {
        await WithUnitOfWorkAsync(async () =>
        {
            await Should.ThrowAsync<EntityNotFoundException>(() => estimateAppService.ApproveAsync(Guid.NewGuid()));
        });
    }

    [Fact]
    public async Task RejectAsync_Should_Throw_UserFriendlyException_When_Reason_Is_Empty()
    {
        await WithUnitOfWorkAsync(async () =>
        {
            await Should.ThrowAsync<UserFriendlyException>(() =>
                estimateAppService.RejectAsync(Guid.NewGuid(), new EstimateRejectDto { Reason = "" }));
        });
    }

    [Fact]
    public async Task ApproveAsync_Should_Throw_When_Reserved_Stock_Is_Insufficient()
    {
        var suffix = Guid.NewGuid().ToString("N")[..8];
        var customer = new Customer($"Cliente {suffix}", $"{suffix}@mail.com", "11999999999", new Document($"DOC{suffix}"));
        var vehicle = new Vehicle("Renault", "Kwid", 2022, $"PQR{suffix}");
        var product = new Product($"Produto {suffix}", 40m);

        await WithUnitOfWorkAsync(async () =>
        {
            await customerRepository.InsertAsync(customer, autoSave: true);
            await vehicleRepository.InsertAsync(vehicle, autoSave: true);
            await productRepository.InsertAsync(product, autoSave: true);

            var inventory = new Inventory(product, 10);
            inventory.ReserveStock(1);
            await inventoryRepository.InsertAsync(inventory, autoSave: true);

            var estimate = new Estimate(Guid.NewGuid(), $"EST-{suffix}", customer.Id, vehicle.Id);
            estimate.AddPartItem(product, 2);
            estimate.SendToCustomer();
            await estimateRepository.InsertAsync(estimate, autoSave: true);

            await Should.ThrowAsync<InvalidOperationException>(() => estimateAppService.ApproveAsync(estimate.Id));
        });
    }

    [Fact]
    public async Task RejectAsync_Should_Throw_UserFriendlyException_When_Estimate_Does_Not_Exist()
    {
        await WithUnitOfWorkAsync(async () =>
        {
            await Should.ThrowAsync<EntityNotFoundException>(() =>
                estimateAppService.RejectAsync(Guid.NewGuid(), new EstimateRejectDto { Reason = "Sem interesse" }));
        });
    }
}