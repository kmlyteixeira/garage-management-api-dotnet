using Shouldly;
using Xunit;

using GarageManagement.Customers;
using GarageManagement.Products;
using GarageManagement.Services;
using GarageManagement.Vehicles;

namespace GarageManagement.Entities.Catalog;

public class CatalogEntityTests
{
    [Fact]
    public void Customer_Should_Preserve_Constructor_Values()
    {
        var document = new Document("12345678901");
        var customer = new Customer("Cliente", "cliente@mail.com", "11999999999", document);

        customer.Name.ShouldBe("Cliente");
        customer.Email.ShouldBe("cliente@mail.com");
        customer.PhoneNumber.ShouldBe("11999999999");
        customer.Document.ShouldBe(document);
    }

    [Fact]
    public void Vehicle_Should_Preserve_Constructor_Values()
    {
        var vehicle = new Vehicle("Ford", "Ka", 2020, "ABC1D23");

        vehicle.Make.ShouldBe("Ford");
        vehicle.Model.ShouldBe("Ka");
        vehicle.Year.ShouldBe(2020);
        vehicle.LicensePlate.ShouldBe("ABC1D23");
    }

    [Fact]
    public void Product_Should_Preserve_Constructor_Values()
    {
        var product = new Product("Filtro", 35m);

        product.Description.ShouldBe("Filtro");
        product.Price.ShouldBe(35m);
    }

    [Fact]
    public void Service_Should_Preserve_Constructor_Values()
    {
        var service = new Service("Troca de óleo", 120m, 1.5);

        service.Description.ShouldBe("Troca de óleo");
        service.Price.ShouldBe(120m);
        service.EstimatedTime.ShouldBe(1.5);
    }
}