using System;
using GarageManagement.Estimates;
using Volo.Abp.Domain.Entities;

namespace GarageManagement.ServiceOrders;

public class ServiceOrder : AggregateRoot<Guid>
{
    public string ServiceOrderNumber { get; private set; }
    public Guid? EstimateId { get; private set; }
    public virtual Estimate? Estimate { get; private set; }
    public ServiceOrderStatus Status { get; private set; }
    public DateTime CreatedAt { get; private set; }
    public DateTime? DiagnosisStartedAt { get; private set; }
    public DateTime? ExecutionStartedAt { get; private set; }
    public DateTime? FinishedAt { get; private set; }
    public DateTime? DeliveredAt { get; private set; }
    public DateTime? ClosedAt { get; private set; }
    public string? CancellationReason { get; private set; }
    public Guid CustomerId { get; private set; }
    public Guid VehicleId { get; private set; }

    private ServiceOrder()
    {
        ServiceOrderNumber = string.Empty;
    }

    public ServiceOrder(Guid id, string serviceOrderNumber, Guid customerId, Guid vehicleId)
        : base(id)
    {
        if (string.IsNullOrWhiteSpace(serviceOrderNumber))
        {
            throw new ArgumentException("Service order number is required.", nameof(serviceOrderNumber));
        }

        ServiceOrderNumber = serviceOrderNumber;
        CustomerId = customerId;
        VehicleId = vehicleId;
        Status = ServiceOrderStatus.Received;
        CreatedAt = DateTime.UtcNow;
    }

    public void StartDiagnosis()
    {
        if (Status != ServiceOrderStatus.Received)
        {
            throw new InvalidOperationException("Diagnosis can only start when service order is received.");
        }

        Status = ServiceOrderStatus.InDiagnosis;
        DiagnosisStartedAt = DateTime.UtcNow;
    }

    public void StartExecution()
    {
        if (Status != ServiceOrderStatus.WaitingExecution)
        {
            throw new InvalidOperationException("Execution can only start after estimate is approved.");
        }

        Status = ServiceOrderStatus.InExecution;
        ExecutionStartedAt = DateTime.UtcNow;
    }

    public void WaitApproval()
    {
        if (!EstimateId.HasValue)
        {
            throw new InvalidOperationException("Service order can only wait approval after diagnosis.");
        }

        Status = ServiceOrderStatus.WaitingApproval;
    }

    public void WaitExecution()
    {
        if (Status != ServiceOrderStatus.WaitingApproval)
        {
            throw new InvalidOperationException("Service order can only wait execution after waiting approval.");
        }

        Status = ServiceOrderStatus.WaitingExecution;
    }

    public void Finish()
    {
        EnsureExecutionStarted();

        Status = ServiceOrderStatus.Finished;
        FinishedAt = DateTime.UtcNow;
    }

    public void Deliver()
    {
        if (Status != ServiceOrderStatus.Finished)
        {
            throw new InvalidOperationException("Service order can only be delivered after it is finished.");
        }

        Status = ServiceOrderStatus.Delivered;
        DeliveredAt = DateTime.UtcNow;
    }

    public void Close()
    {
        if (Status != ServiceOrderStatus.Delivered)
        {
            throw new InvalidOperationException("Service order can only be closed after it is delivered.");
        }

        Status = ServiceOrderStatus.Closed;
        ClosedAt = DateTime.UtcNow;
    }

    public void Cancel(string reason)
    {
        if (Status == ServiceOrderStatus.Closed)
        {
            throw new InvalidOperationException("Closed service order cannot be canceled.");
        }

        if (string.IsNullOrWhiteSpace(reason))
        {
            throw new ArgumentException("Cancellation reason is required.", nameof(reason));
        }

        Status = ServiceOrderStatus.Canceled;
        CancellationReason = reason;
    }

    public void ChangeStatus(ServiceOrderStatus status)
    {
        switch (status)
        {
            case ServiceOrderStatus.InDiagnosis:
                StartDiagnosis();
                return;
            case ServiceOrderStatus.InExecution:
                StartExecution();
                return;
            case ServiceOrderStatus.WaitingApproval:
                WaitApproval();
                return;
            case ServiceOrderStatus.WaitingExecution:
                WaitExecution();
                return;
            case ServiceOrderStatus.Finished:
                Finish();
                return;
            case ServiceOrderStatus.Delivered:
                Deliver();
                return;
            case ServiceOrderStatus.Closed:
                Close();
                return;
            case ServiceOrderStatus.Canceled:
                throw new InvalidOperationException("Service order cancellation requires a reason.");
            case ServiceOrderStatus.Received:
            default:
                throw new InvalidOperationException("Requested service order status transition is not supported.");
        }
    }

    public void AssociateEstimate(Guid estimateId)
    {
        if (EstimateId.HasValue)
        {
            throw new InvalidOperationException("Service order already has an associated estimate.");
        }

        EstimateId = estimateId;
    }

    private void EnsureExecutionStarted()
    {
        if (Status != ServiceOrderStatus.InExecution)
        {
            throw new InvalidOperationException("Service order must be in execution.");
        }
    }
}
