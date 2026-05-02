using System;
using GarageManagement.Estimates;
using GarageManagement.Products;
using GarageManagement.Services;
using Shouldly;
using Xunit;

namespace GarageManagement.Entities.Estimates;

public class EstimateItemTests
{
    [Fact]
    public void Service_Item_Should_Calculate_Total_And_Update_Details()
    {
        var estimate = new Estimate(Guid.NewGuid(), "EST-ITEM-1", Guid.NewGuid(), Guid.NewGuid());
        var service = new Service("Troca de óleo", 120m, 1.5);
        var updatedService = new Service("Troca de óleo premium", 150m, 2.0);

        var item = new EstimateServiceItem(Guid.NewGuid(), estimate, service, 2);

        item.TotalPrice.ShouldBe(240m);

        item.UpdateDetails(updatedService, 3);

        item.ServiceId.ShouldBe(updatedService.Id);
        item.Quantity.ShouldBe(3);
        item.TotalPrice.ShouldBe(450m);
    }

    [Fact]
    public void Product_Item_Should_Calculate_Total_And_Update_Details()
    {
        var estimate = new Estimate(Guid.NewGuid(), "EST-ITEM-2", Guid.NewGuid(), Guid.NewGuid());
        var product = new Product("Filtro", 35m);
        var updatedProduct = new Product("Filtro esportivo", 50m);

        var item = new EstimateProductItem(Guid.NewGuid(), estimate, product, 4);

        item.TotalPrice.ShouldBe(140m);

        item.UpdateDetails(updatedProduct, 2);

        item.ProductId.ShouldBe(updatedProduct.Id);
        item.Quantity.ShouldBe(2);
        item.TotalPrice.ShouldBe(100m);
    }

    [Theory]
    [InlineData(0)]
    [InlineData(-1)]
    public void Service_Item_Should_Reject_Invalid_Quantity(int quantity)
    {
        var estimate = new Estimate(Guid.NewGuid(), "EST-ITEM-3", Guid.NewGuid(), Guid.NewGuid());
        var service = new Service("Alinhamento", 90m, 1.0);

        Should.Throw<ArgumentException>(() => new EstimateServiceItem(Guid.NewGuid(), estimate, service, quantity));
        var item = new EstimateServiceItem(Guid.NewGuid(), estimate, service, 1);
        Should.Throw<ArgumentException>(() => item.UpdateDetails(service, quantity));
    }

    [Theory]
    [InlineData(0)]
    [InlineData(-1)]
    public void Product_Item_Should_Reject_Invalid_Quantity(int quantity)
    {
        var estimate = new Estimate(Guid.NewGuid(), "EST-ITEM-4", Guid.NewGuid(), Guid.NewGuid());
        var product = new Product("Palheta", 40m);

        Should.Throw<ArgumentException>(() => new EstimateProductItem(Guid.NewGuid(), estimate, product, quantity));
        var item = new EstimateProductItem(Guid.NewGuid(), estimate, product, 1);
        Should.Throw<ArgumentException>(() => item.UpdateDetails(product, quantity));
    }
}