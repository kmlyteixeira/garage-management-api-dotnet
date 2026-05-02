using System;
using System.Threading.Tasks;
using GarageManagement.Customers;
using GarageManagement.EntityFrameworkCore.TestDoubles;
using GarageManagement.Estimates;
using GarageManagement.Inventories;
using GarageManagement.Products;
using GarageManagement.Services;
using GarageManagement.ServiceOrders;
using GarageManagement.Vehicles;
using Shouldly;
using Volo.Abp;
using Volo.Abp.Domain.Repositories;
using Xunit;

namespace GarageManagement.EntityFrameworkCore.Applications;

[Collection(GarageManagementTestConsts.CollectionDefinitionName)]
public class ServiceOrderUpdateHandlerIntegrationTests : GarageManagementEntityFrameworkCoreTestBase
{
    private readonly IServiceOrderUpdateHandler serviceOrderUpdateHandler;
    private readonly IRepository<Customer, Guid> customerRepository;
    private readonly IRepository<Vehicle, Guid> vehicleRepository;
    private readonly IRepository<Service, Guid> serviceRepository;
    private readonly IRepository<Product, Guid> productRepository;
    private readonly IRepository<Inventory, Guid> inventoryRepository;
    private readonly IRepository<ServiceOrder, Guid> serviceOrderRepository;
    private readonly IRepository<Estimate, Guid> estimateRepository;
    private readonly TestEmailSender testEmailSender;

    public ServiceOrderUpdateHandlerIntegrationTests()
    {
        serviceOrderUpdateHandler = GetRequiredService<IServiceOrderUpdateHandler>();
        customerRepository = GetRequiredService<IRepository<Customer, Guid>>();
        vehicleRepository = GetRequiredService<IRepository<Vehicle, Guid>>();
        serviceRepository = GetRequiredService<IRepository<Service, Guid>>();
        productRepository = GetRequiredService<IRepository<Product, Guid>>();
        inventoryRepository = GetRequiredService<IRepository<Inventory, Guid>>();
        serviceOrderRepository = GetRequiredService<IRepository<ServiceOrder, Guid>>();
        estimateRepository = GetRequiredService<IRepository<Estimate, Guid>>();
        testEmailSender = GetRequiredService<TestEmailSender>();
    }

    [Fact]
    public async Task HandleAsync_Should_Create_Estimate_Promote_Status_And_Reserve_Stock()
    {
        testEmailSender.Clear();
        var suffix = Guid.NewGuid().ToString("N")[..8];
        var document = new Document($"123456789{suffix[..2]}");
        var customer = new Customer($"Cliente {suffix}", $"cliente-{suffix}@mail.com", "11999999999", document);
        var vehicle = new Vehicle("Ford", "Ka", 2020, $"ABC{suffix[..3]}1");
        var service = new Service("Troca de óleo", 120m, 1.5);
        var product = new Product("Filtro", 35m);

        await WithUnitOfWorkAsync(async () =>
        {
            await customerRepository.InsertAsync(customer, autoSave: true);
            await vehicleRepository.InsertAsync(vehicle, autoSave: true);
            await serviceRepository.InsertAsync(service, autoSave: true);
            await productRepository.InsertAsync(product, autoSave: true);
            await inventoryRepository.InsertAsync(new Inventory(product, 10), autoSave: true);

            var serviceOrder = new ServiceOrder(Guid.NewGuid(), $"OS-{suffix}", customer.Id, vehicle.Id);
            serviceOrder.StartDiagnosis();
            await serviceOrderRepository.InsertAsync(serviceOrder, autoSave: true);

            var result = await serviceOrderUpdateHandler.HandleAsync(serviceOrder, new ServiceOrderUpdateDto
            {
                ServiceItems = new()
                {
                    new ServiceOrderServiceItemCreateDto { ServiceId = service.Id, Quantity = 2 }
                },
                PartItems = new()
                {
                    new ServiceOrderProductItemCreateDto { ProductId = product.Id, Quantity = 3 }
                }
            });

            var updatedOrder = await serviceOrderRepository.GetAsync(serviceOrder.Id);
            var updatedEstimate = await estimateRepository.GetAsync(updatedOrder.EstimateId!.Value, includeDetails: true);

            result.Status.ShouldBe(ServiceOrderStatus.WaitingApproval);
            updatedOrder.Status.ShouldBe(ServiceOrderStatus.WaitingApproval);
            updatedEstimate.Status.ShouldBe(EstimateStatus.PendingApproval);
            updatedEstimate.TotalAmount.ShouldBe(345m);
            testEmailSender.SentEmails.Count.ShouldBe(1);
        });
    }

    [Fact]
    public async Task HandleAsync_Should_Keep_Draft_When_There_Is_No_Stock()
    {
        testEmailSender.Clear();
        var suffix = Guid.NewGuid().ToString("N")[..8];
        var customer = new Customer($"Cliente {suffix}", $"cliente-{suffix}@mail.com", "11999999999", new Document($"123456789{suffix[..2]}"));
        var vehicle = new Vehicle("VW", "Gol", 2019, $"DEF{suffix[..3]}2");
        var service = new Service("Alinhamento", 90m, 1.0);
        var product = new Product("Pastilha", 100m);

        await WithUnitOfWorkAsync(async () =>
        {
            await customerRepository.InsertAsync(customer, autoSave: true);
            await vehicleRepository.InsertAsync(vehicle, autoSave: true);
            await serviceRepository.InsertAsync(service, autoSave: true);
            await productRepository.InsertAsync(product, autoSave: true);
            await inventoryRepository.InsertAsync(new Inventory(product, 1), autoSave: true);

            var serviceOrder = new ServiceOrder(Guid.NewGuid(), $"OS-{suffix}", customer.Id, vehicle.Id);
            serviceOrder.StartDiagnosis();
            await serviceOrderRepository.InsertAsync(serviceOrder, autoSave: true);

            await serviceOrderUpdateHandler.HandleAsync(serviceOrder, new ServiceOrderUpdateDto
            {
                ServiceItems = new()
                {
                    new ServiceOrderServiceItemCreateDto { ServiceId = service.Id, Quantity = 1 }
                },
                PartItems = new()
                {
                    new ServiceOrderProductItemCreateDto { ProductId = product.Id, Quantity = 2 }
                }
            });

            var updatedOrder = await serviceOrderRepository.GetAsync(serviceOrder.Id);
            var updatedEstimate = await estimateRepository.GetAsync(updatedOrder.EstimateId!.Value, includeDetails: true);
            var updatedInventory = await inventoryRepository.GetAsync(item => item.ProductId == product.Id);

            updatedOrder.Status.ShouldBe(ServiceOrderStatus.InDiagnosis);
            updatedEstimate.Status.ShouldBe(EstimateStatus.Draft);
            updatedInventory.Quantity.ShouldBe(1);
            updatedInventory.ReservedQuantity.ShouldBe(0);
            testEmailSender.SentEmails.Count.ShouldBe(0);
        });
    }

    [Fact]
    public async Task HandleAsync_Should_Throw_When_ServiceOrder_Is_Not_In_Diagnosis()
    {
        testEmailSender.Clear();
        var serviceOrder = new ServiceOrder(Guid.NewGuid(), "OS-INVALID", Guid.NewGuid(), Guid.NewGuid());

        await Should.ThrowAsync<UserFriendlyException>(() => serviceOrderUpdateHandler.HandleAsync(serviceOrder, new ServiceOrderUpdateDto()));
    }

    [Fact]
    public async Task HandleAsync_Should_Throw_When_Service_Is_Invalid()
    {
        testEmailSender.Clear();
        var suffix = Guid.NewGuid().ToString("N")[..8];
        var customer = new Customer($"Cliente {suffix}", $"cliente-{suffix}@mail.com", "11999999999", new Document($"123456789{suffix[..2]}"));
        var vehicle = new Vehicle("Fiat", "Argo", 2022, $"GHI{suffix[..3]}3");

        await WithUnitOfWorkAsync(async () =>
        {
            await customerRepository.InsertAsync(customer, autoSave: true);
            await vehicleRepository.InsertAsync(vehicle, autoSave: true);

            var serviceOrder = new ServiceOrder(Guid.NewGuid(), $"OS-{suffix}", customer.Id, vehicle.Id);
            serviceOrder.StartDiagnosis();
            await serviceOrderRepository.InsertAsync(serviceOrder, autoSave: true);

            await Should.ThrowAsync<UserFriendlyException>(() => serviceOrderUpdateHandler.HandleAsync(serviceOrder, new ServiceOrderUpdateDto
            {
                ServiceItems = new()
                {
                    new ServiceOrderServiceItemCreateDto { ServiceId = Guid.NewGuid(), Quantity = 1 }
                }
            }));
        });
    }

    [Fact]
    public async Task HandleAsync_Should_Throw_When_Product_Is_Invalid()
    {
        testEmailSender.Clear();
        var suffix = Guid.NewGuid().ToString("N")[..8];
        var customer = new Customer($"Cliente {suffix}", $"cliente-{suffix}@mail.com", "11999999999", new Document($"123456789{suffix[..2]}"));
        var vehicle = new Vehicle("Hyundai", "HB20", 2021, $"JKL{suffix[..3]}4");
        var service = new Service("Revisão", 200m, 2.0);

        await WithUnitOfWorkAsync(async () =>
        {
            await customerRepository.InsertAsync(customer, autoSave: true);
            await vehicleRepository.InsertAsync(vehicle, autoSave: true);
            await serviceRepository.InsertAsync(service, autoSave: true);

            var serviceOrder = new ServiceOrder(Guid.NewGuid(), $"OS-{suffix}", customer.Id, vehicle.Id);
            serviceOrder.StartDiagnosis();
            await serviceOrderRepository.InsertAsync(serviceOrder, autoSave: true);

            await Should.ThrowAsync<UserFriendlyException>(() => serviceOrderUpdateHandler.HandleAsync(serviceOrder, new ServiceOrderUpdateDto
            {
                ServiceItems = new()
                {
                    new ServiceOrderServiceItemCreateDto { ServiceId = service.Id, Quantity = 1 }
                },
                PartItems = new()
                {
                    new ServiceOrderProductItemCreateDto { ProductId = Guid.NewGuid(), Quantity = 1 }
                }
            }));
        });
    }
}