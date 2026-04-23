using System;
using GarageManagement.Inventories;
using GarageManagement.Products;
using Shouldly;
using Xunit;

namespace GarageManagement.Entities.Inventories;

public class InventoryTests
{
    [Fact]
    public void Reserve_And_Consume_Should_Update_Quantity_And_ReservedQuantity()
    {
        var inventory = new Inventory(new Product("Oleo 5W30", 50m), 10);

        inventory.ReserveStock(4);
        inventory.ConsumeReservedStock(3);

        inventory.Quantity.ShouldBe(7);
        inventory.ReservedQuantity.ShouldBe(1);
    }

    [Fact]
    public void Reserve_More_Than_Available_Should_Throw()
    {
        var inventory = new Inventory(new Product("Correia", 120m), 5);

        Should.Throw<InvalidOperationException>(() => inventory.ReserveStock(6));
    }

    [Fact]
    public void Release_More_Than_Reserved_Should_Throw()
    {
        var inventory = new Inventory(new Product("Bateria", 400m), 10);
        inventory.ReserveStock(3);

        Should.Throw<InvalidOperationException>(() => inventory.ReleaseReservedStock(4));
    }

    [Fact]
    public void Decrease_Stock_Should_Consider_Reserved_Quantity()
    {
        var inventory = new Inventory(new Product("Pneu", 350m), 10);
        inventory.ReserveStock(8);

        Should.Throw<InvalidOperationException>(() => inventory.DecreaseStock(3));
    }
}