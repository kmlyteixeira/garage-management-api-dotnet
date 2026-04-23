using System;
using GarageManagement.ServiceOrders;
using Shouldly;
using Xunit;

namespace GarageManagement.Entities.ServiceOrders;

public class ServiceOrderTests
{
    [Fact]
    public void Happy_Path_Should_Reach_Closed()
    {
        var serviceOrder = new ServiceOrder(Guid.NewGuid(), "OS-001", Guid.NewGuid(), Guid.NewGuid());

        serviceOrder.AssociateEstimate(Guid.NewGuid());
        serviceOrder.StartDiagnosis();
        serviceOrder.WaitApproval();
        serviceOrder.WaitExecution();
        serviceOrder.StartExecution();
        serviceOrder.Finish();
        serviceOrder.Deliver();
        serviceOrder.Close();

        serviceOrder.Status.ShouldBe(ServiceOrderStatus.Closed);
        serviceOrder.DiagnosisStartedAt.ShouldNotBeNull();
        serviceOrder.ExecutionStartedAt.ShouldNotBeNull();
        serviceOrder.FinishedAt.ShouldNotBeNull();
        serviceOrder.DeliveredAt.ShouldNotBeNull();
        serviceOrder.ClosedAt.ShouldNotBeNull();
    }

    [Fact]
    public void WaitApproval_Without_Estimate_Should_Throw()
    {
        var serviceOrder = new ServiceOrder(Guid.NewGuid(), "OS-002", Guid.NewGuid(), Guid.NewGuid());

        Should.Throw<InvalidOperationException>(() => serviceOrder.WaitApproval());
    }

    [Fact]
    public void Closed_Order_Should_Not_Be_Canceled()
    {
        var serviceOrder = new ServiceOrder(Guid.NewGuid(), "OS-003", Guid.NewGuid(), Guid.NewGuid());
        serviceOrder.AssociateEstimate(Guid.NewGuid());
        serviceOrder.StartDiagnosis();
        serviceOrder.WaitApproval();
        serviceOrder.WaitExecution();
        serviceOrder.StartExecution();
        serviceOrder.Finish();
        serviceOrder.Deliver();
        serviceOrder.Close();

        Should.Throw<InvalidOperationException>(() => serviceOrder.Cancel("Nao pode cancelar"));
    }

    [Fact]
    public void ChangeStatus_To_Canceled_Should_Require_Reason()
    {
        var serviceOrder = new ServiceOrder(Guid.NewGuid(), "OS-004", Guid.NewGuid(), Guid.NewGuid());

        Should.Throw<InvalidOperationException>(() => serviceOrder.ChangeStatus(ServiceOrderStatus.Canceled));
    }
}