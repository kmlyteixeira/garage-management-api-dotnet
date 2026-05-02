using System;
using Shouldly;
using Xunit;

namespace GarageManagement.ServiceOrders.Tests;

public class ServiceOrderTests
{
    [Fact]
    public void Constructor_Should_Require_Number()
    {
        Should.Throw<ArgumentException>(() => new ServiceOrder(Guid.NewGuid(), "", Guid.NewGuid(), Guid.NewGuid()));
    }

    [Fact]
    public void StartExecution_When_Not_WaitingExecution_Should_Throw()
    {
        var so = new ServiceOrder(Guid.NewGuid(), "SO-1", Guid.NewGuid(), Guid.NewGuid());

        Should.Throw<InvalidOperationException>(() => so.StartExecution());
    }

    [Fact]
    public void WaitApproval_Without_Estimate_Should_Throw()
    {
        var so = new ServiceOrder(Guid.NewGuid(), "SO-1", Guid.NewGuid(), Guid.NewGuid());

        Should.Throw<InvalidOperationException>(() => so.WaitApproval());
    }

    [Fact]
    public void Finish_Without_InExecution_Should_Throw()
    {
        var so = new ServiceOrder(Guid.NewGuid(), "SO-1", Guid.NewGuid(), Guid.NewGuid());

        Should.Throw<InvalidOperationException>(() => so.Finish());
    }

    [Fact]
    public void Cancel_Without_Reason_Should_Throw()
    {
        var so = new ServiceOrder(Guid.NewGuid(), "SO-1", Guid.NewGuid(), Guid.NewGuid());

        Should.Throw<ArgumentException>(() => so.Cancel("  "));
    }

    [Fact]
    public void AssociateEstimate_When_Already_Associated_Should_Throw()
    {
        var so = new ServiceOrder(Guid.NewGuid(), "SO-1", Guid.NewGuid(), Guid.NewGuid());
        so.AssociateEstimate(Guid.NewGuid());

        Should.Throw<InvalidOperationException>(() => so.AssociateEstimate(Guid.NewGuid()));
    }

    [Fact]
    public void ChangeStatus_Canceled_Should_Throw_Without_Reason()
    {
        var so = new ServiceOrder(Guid.NewGuid(), "SO-1", Guid.NewGuid(), Guid.NewGuid());

        Should.Throw<InvalidOperationException>(() => so.ChangeStatus(ServiceOrderStatus.Canceled));
    }

    [Fact]
    public void ChangeStatus_Unsupported_Should_Throw()
    {
        var so = new ServiceOrder(Guid.NewGuid(), "SO-1", Guid.NewGuid(), Guid.NewGuid());

        Should.Throw<InvalidOperationException>(() => so.ChangeStatus(ServiceOrderStatus.Received));
    }
}
