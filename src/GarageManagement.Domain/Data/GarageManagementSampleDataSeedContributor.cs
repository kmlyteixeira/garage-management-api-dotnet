using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using GarageManagement.Customers;
using GarageManagement.Inventories;
using GarageManagement.Products;
using GarageManagement.Services;
using GarageManagement.Vehicles;
using Volo.Abp.Data;
using Volo.Abp.DependencyInjection;
using Volo.Abp.Domain.Repositories;

namespace GarageManagement.Data;

public class GarageManagementSampleDataSeedContributor : IDataSeedContributor, ITransientDependency
{
    private readonly IRepository<Customer, Guid> _customerRepository;
    private readonly IRepository<Vehicle, Guid> _vehicleRepository;
    private readonly IRepository<Product, Guid> _productRepository;
    private readonly IRepository<Service, Guid> _serviceRepository;
    private readonly IRepository<Inventory, Guid> _inventoryRepository;

    public GarageManagementSampleDataSeedContributor(
        IRepository<Customer, Guid> customerRepository,
        IRepository<Vehicle, Guid> vehicleRepository,
        IRepository<Product, Guid> productRepository,
        IRepository<Service, Guid> serviceRepository,
        IRepository<Inventory, Guid> inventoryRepository)
    {
        _customerRepository = customerRepository;
        _vehicleRepository = vehicleRepository;
        _productRepository = productRepository;
        _serviceRepository = serviceRepository;
        _inventoryRepository = inventoryRepository;
    }

    public async Task SeedAsync(DataSeedContext context)
    {
        await SeedCustomersAsync();
        await SeedVehiclesAsync();
        var products = await SeedProductsAsync();
        await SeedServicesAsync();
        await SeedInventoriesAsync(products);
    }

    private async Task SeedCustomersAsync()
    {
        if (await _customerRepository.GetCountAsync() > 0)
            return;

        var customers = new List<Customer>
        {
            new("Carlos Silva",       "carlos.silva@email.com",    "11987654321", new Document("52998224725")),
            new("Ana Pereira",        "ana.pereira@email.com",     "21976543210", new Document("61821614290")),
            new("Roberto Almeida",    "roberto.almeida@email.com", "31965432109", new Document("11144477735")),
            new("Fernanda Costa",     "fernanda.costa@email.com",  "41954321098", new Document("22255588846")),
            new("Paulo Souza",        "paulo.souza@email.com",     "51943210987", new Document("33366699957")),
            new("Juliana Lima",       "juliana.lima@email.com",    "61932109876", new Document("44477700068")),
            new("Marcelo Ferreira",   "marcelo.ferreira@email.com","71921098765", new Document("55588811179")),
            new("Beatriz Santos",     "beatriz.santos@email.com",  "81910987654", new Document("66699922280")),
            new("Thiago Oliveira",    "thiago.oliveira@email.com", "91909876543", new Document("77700033391")),
            new("Camila Rodrigues",   "camila.rodrigues@email.com","11898765432", new Document("88811144402")),
        };

        await _customerRepository.InsertManyAsync(customers, autoSave: true);
    }

    private async Task SeedVehiclesAsync()
    {
        if (await _vehicleRepository.GetCountAsync() > 0)
            return;

        var vehicles = new List<Vehicle>
        {
            new("Toyota",      "Corolla", 2020, "ABC1D23"),
            new("Honda",       "Civic",   2019, "DEF2E34"),
            new("Volkswagen",  "Golf",    2021, "GHI3F45"),
            new("Ford",        "Ka",      2018, "JKL4G56"),
            new("Chevrolet",   "Onix",    2022, "MNO5H67"),
            new("Hyundai",     "HB20",    2020, "PQR6I78"),
            new("Renault",     "Kwid",    2021, "STU7J89"),
            new("Nissan",      "Versa",   2019, "VWX8K90"),
            new("Fiat",        "Argo",    2023, "YZA9L01"),
            new("Jeep",        "Compass", 2022, "BCD0M12"),
        };

        await _vehicleRepository.InsertManyAsync(vehicles, autoSave: true);
    }

    private async Task<List<Product>> SeedProductsAsync()
    {
        if (await _productRepository.GetCountAsync() > 0)
            return await _productRepository.GetListAsync();

        var products = new List<Product>
        {
            new("Óleo Motor 5W30 Sintético 1L",        45.90m),
            new("Filtro de Óleo",                      28.50m),
            new("Filtro de Ar",                        35.00m),
            new("Pastilha de Freio Dianteira (par)",   89.90m),
            new("Correia Dentada",                    120.00m),
            new("Vela de Ignição (kit 4un)",           95.00m),
            new("Fluido de Freio DOT4 500ml",          22.00m),
            new("Amortecedor Dianteiro",               210.00m),
            new("Lâmpada Farol H7",                    18.50m),
            new("Bateria Automotiva 60Ah",            380.00m),
        };

        await _productRepository.InsertManyAsync(products, autoSave: true);
        return products;
    }

    private async Task SeedServicesAsync()
    {
        if (await _serviceRepository.GetCountAsync() > 0)
            return;

        var services = new List<Service>
        {
            new("Troca de Óleo e Filtro",              80.00m, 0.5),
            new("Alinhamento e Balanceamento",        120.00m, 1.0),
            new("Revisão Geral",                      250.00m, 3.0),
            new("Troca de Pastilha de Freio",         100.00m, 1.5),
            new("Diagnóstico Eletrônico",             150.00m, 1.0),
            new("Troca de Correia Dentada",           350.00m, 4.0),
            new("Higienização do Ar-Condicionado",    180.00m, 2.0),
            new("Geometria (Cambagem e Caster)",      200.00m, 2.5),
            new("Troca de Amortecedores",             400.00m, 3.0),
            new("Polimento e Vitrificação",           600.00m, 5.0),
        };

        await _serviceRepository.InsertManyAsync(services, autoSave: true);
    }

    private async Task SeedInventoriesAsync(List<Product> products)
    {
        if (await _inventoryRepository.GetCountAsync() > 0)
            return;

        var quantities = new[] { 50, 100, 80, 40, 30, 60, 120, 15, 200, 10 };

        var inventories = new List<Inventory>();
        for (var i = 0; i < products.Count; i++)
            inventories.Add(new Inventory(products[i], quantities[i]));

        await _inventoryRepository.InsertManyAsync(inventories, autoSave: true);
    }
}
