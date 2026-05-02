using System;
using Shouldly;
using Xunit;

namespace GarageManagement.Estimates.Tests;

public class EstimateTests
{
    [Fact]
    public void Private_Constructor_Should_Initialize_Default_State()
    {
        var estimate = (Estimate)Activator.CreateInstance(typeof(Estimate), nonPublic: true)!;

        estimate.EstimateNumber.ShouldBe(string.Empty);
        estimate.ServiceItems.ShouldNotBeNull();
        estimate.PartItems.ShouldNotBeNull();
    }

    [Fact]
    public void AddServiceItem_When_Approved_Should_Throw()
    {
        var estimate = new Estimate(Guid.NewGuid(), "E-1", Guid.NewGuid(), Guid.NewGuid());
        estimate.SendToCustomer();
        estimate.Approve();

        var service = new GarageManagement.Services.Service("S", 100m, 1);

        Should.Throw<InvalidOperationException>(() => estimate.AddServiceItem(service, 1));
    }

    [Fact]
    public void UpdateServiceItem_When_Not_Found_Should_Throw()
    {
        var estimate = new Estimate(Guid.NewGuid(), "E-1", Guid.NewGuid(), Guid.NewGuid());
        var service = new GarageManagement.Services.Service("S", 100m, 1);

        Should.Throw<InvalidOperationException>(() => estimate.UpdateServiceItem(service, 1));
    }

    [Fact]
    public void SendToCustomer_When_Not_Draft_Should_Throw()
    {
        var estimate = new Estimate(Guid.NewGuid(), "E-1", Guid.NewGuid(), Guid.NewGuid());
        estimate.SendToCustomer();

        Should.Throw<InvalidOperationException>(() => estimate.SendToCustomer());
    }

    [Fact]
    public void Approve_When_Not_PendingApproval_Should_Throw()
    {
        var estimate = new Estimate(Guid.NewGuid(), "E-1", Guid.NewGuid(), Guid.NewGuid());

        Should.Throw<InvalidOperationException>(() => estimate.Approve());
    }

    [Fact]
    public void Reject_When_Not_PendingApproval_Should_Throw()
    {
        var estimate = new Estimate(Guid.NewGuid(), "E-1", Guid.NewGuid(), Guid.NewGuid());

        Should.Throw<InvalidOperationException>(() => estimate.Reject("any reason"));
    }

    [Fact]
    public void AddPartItem_Update_Remove_Workflow_Recalculates_Total()
    {
        var estimate = new Estimate(Guid.NewGuid(), "E-1", Guid.NewGuid(), Guid.NewGuid());
        var product = new GarageManagement.Products.Product("P", 10m);

        estimate.AddPartItem(product, 2);
        estimate.TotalAmount.ShouldBe(20m);

        estimate.UpdatePartItem(product, 3);
        estimate.TotalAmount.ShouldBe(30m);

        var itemId = ((System.Collections.Generic.List<GarageManagement.Estimates.EstimateProductItem>)estimate.PartItems)[0].Id;
        estimate.RemovePartItem(itemId);
        estimate.TotalAmount.ShouldBe(0m);
    }

    [Fact]
    public void Reject_Without_Reason_Should_Throw()
    {
        var estimate = new Estimate(Guid.NewGuid(), "E-1", Guid.NewGuid(), Guid.NewGuid());
        estimate.SendToCustomer();

        Should.Throw<ArgumentException>(() => estimate.Reject("  "));
    }
}
