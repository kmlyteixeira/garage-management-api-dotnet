using System;
using GarageManagement.Estimates;
using GarageManagement.Products;
using GarageManagement.Services;
using Shouldly;
using Xunit;

namespace GarageManagement.Entities.Estimates;

public class EstimateTests
{
    [Fact]
    public void Add_Items_Should_Recalculate_Total_Amount()
    {
        var estimate = new Estimate(Guid.NewGuid(), "EST-001", Guid.NewGuid(), Guid.NewGuid());
        var service = new Service("Troca de oleo", 120m, 1.5);
        var product = new Product("Filtro", 35m);

        estimate.AddServiceItem(service, 2);
        estimate.AddPartItem(product, 3);

        estimate.TotalAmount.ShouldBe(345m);
    }

    [Fact]
    public void Send_And_Approve_Should_Change_Status_And_Timestamps()
    {
        var estimate = new Estimate(Guid.NewGuid(), "EST-002", Guid.NewGuid(), Guid.NewGuid());

        estimate.SendToCustomer();
        estimate.Approve();

        estimate.Status.ShouldBe(EstimateStatus.Approved);
        estimate.SentAt.ShouldNotBeNull();
        estimate.ApprovedAt.ShouldNotBeNull();
    }

    [Fact]
    public void Reject_Should_Set_Status_Reason_And_Timestamp()
    {
        var estimate = new Estimate(Guid.NewGuid(), "EST-003", Guid.NewGuid(), Guid.NewGuid());

        estimate.SendToCustomer();
        estimate.Reject("Cliente sem interesse");

        estimate.Status.ShouldBe(EstimateStatus.Rejected);
        estimate.RejectedAt.ShouldNotBeNull();
        estimate.RejectionReason.ShouldBe("Cliente sem interesse");
    }

    [Fact]
    public void Editing_After_Approval_Should_Throw()
    {
        var estimate = new Estimate(Guid.NewGuid(), "EST-004", Guid.NewGuid(), Guid.NewGuid());
        var product = new Product("Pastilha de freio", 90m);

        estimate.SendToCustomer();
        estimate.Approve();

        Should.Throw<InvalidOperationException>(() => estimate.AddPartItem(product, 1));
    }
}