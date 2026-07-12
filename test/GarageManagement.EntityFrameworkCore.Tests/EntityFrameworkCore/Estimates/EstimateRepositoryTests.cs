using System;
using System.Linq;
using System.Threading.Tasks;
using GarageManagement.Customers;
using GarageManagement.EntityFrameworkCore;
using GarageManagement.Estimates;
using GarageManagement.Products;
using GarageManagement.Services;
using GarageManagement.Vehicles;
using Shouldly;
using Volo.Abp.Domain.Entities;
using Volo.Abp.Domain.Repositories;
using Xunit;

namespace GarageManagement.EntityFrameworkCore.Estimates;

[Collection(GarageManagementTestConsts.CollectionDefinitionName)]
public class EstimateRepositoryTests : GarageManagementEntityFrameworkCoreTestBase
{
    private readonly IEstimateRepository estimateRepository;
    private readonly IRepository<Customer, Guid> customerRepository;
    private readonly IRepository<Vehicle, Guid> vehicleRepository;
    private readonly IRepository<Product, Guid> productRepository;
    private readonly IRepository<Service, Guid> serviceRepository;

    public EstimateRepositoryTests()
    {
        estimateRepository = GetRequiredService<IEstimateRepository>();
        customerRepository = GetRequiredService<IRepository<Customer, Guid>>();
        vehicleRepository = GetRequiredService<IRepository<Vehicle, Guid>>();
        productRepository = GetRequiredService<IRepository<Product, Guid>>();
        serviceRepository = GetRequiredService<IRepository<Service, Guid>>();
    }

    [Fact]
    public async Task GetWithDetailsAsync_Should_Load_Service_And_Part_Items_With_Navigations()
    {
        var suffix = Guid.NewGuid().ToString("N")[..8];
        var customer = new Customer($"Cliente {suffix}", $"{suffix}@mail.com", "11999999999", new Document($"DOC{suffix}"));
        var vehicle = new Vehicle("VW", "Gol", 2020, $"ABC{suffix}");
        var service = new Service($"Alinhamento {suffix}", 120m, 1.5);
        var product = new Product($"Filtro {suffix}", 35m);

        await WithUnitOfWorkAsync(async () =>
        {
            await customerRepository.InsertAsync(customer, autoSave: true);
            await vehicleRepository.InsertAsync(vehicle, autoSave: true);
            await serviceRepository.InsertAsync(service, autoSave: true);
            await productRepository.InsertAsync(product, autoSave: true);

            var estimate = new Estimate(Guid.NewGuid(), $"EST-{suffix}", customer.Id, vehicle.Id);
            estimate.AddServiceItem(service, 2);
            estimate.AddPartItem(product, 3);

            await estimateRepository.InsertAsync(estimate, autoSave: true);

            var result = await estimateRepository.GetWithDetailsAsync(estimate.Id);

            result.ServiceItems.ShouldHaveSingleItem();
            result.PartItems.ShouldHaveSingleItem();
            result.ServiceItems.First().Service.ShouldNotBeNull();
            result.PartItems.First().Product.ShouldNotBeNull();
            result.ServiceItems.First().Quantity.ShouldBe(2);
            result.PartItems.First().Quantity.ShouldBe(3);
        });
    }

    [Fact]
    public async Task GetWithDetailsAsync_Should_Throw_When_Estimate_Is_Missing()
    {
        await WithUnitOfWorkAsync(async () =>
        {
            await Should.ThrowAsync<EntityNotFoundException>(() => estimateRepository.GetWithDetailsAsync(Guid.NewGuid()));
        });
    }
}