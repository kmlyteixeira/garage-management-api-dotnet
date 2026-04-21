using System;
using System.Collections.Generic;
using System.Linq;
using GarageManagement.Products;
using GarageManagement.Services;
using Volo.Abp.Domain.Entities;

namespace GarageManagement.Estimates;

public class Estimate : AggregateRoot<Guid>
{
    public string EstimateNumber { get; private set; }
    public Guid CustomerId { get; private set; }
    public Guid VehicleId { get; private set; }
    public EstimateStatus Status { get; private set; }
    public decimal TotalAmount { get; private set; }
    public DateTime CreatedAt { get; private set; }
    public DateTime? SentAt { get; private set; }
    public DateTime? ApprovedAt { get; private set; }
    public DateTime? RejectedAt { get; private set; }
    public string? RejectionReason { get; private set; }
    public Guid? ConvertedToServiceOrderId { get; private set; }

    public ICollection<EstimateServiceItem> ServiceItems { get; private set; }
    public ICollection<EstimateProductItem> PartItems { get; private set; }

    private Estimate()
    {
        EstimateNumber = string.Empty;
        ServiceItems = new List<EstimateServiceItem>();
        PartItems = new List<EstimateProductItem>();
    }

    public Estimate(Guid id, string estimateNumber, Guid customerId, Guid vehicleId) : base(id)
    {
        if (string.IsNullOrWhiteSpace(estimateNumber))
        {
            throw new ArgumentException("Estimate number is required.", nameof(estimateNumber));
        }

        EstimateNumber = estimateNumber;
        CustomerId = customerId;
        VehicleId = vehicleId;
        Status = EstimateStatus.Draft;
        CreatedAt = DateTime.UtcNow;
        ServiceItems = new List<EstimateServiceItem>();
        PartItems = new List<EstimateProductItem>();
    }

    public void AddServiceItem(Service service, int quantity)
    {
        EnsureEditable();

        var item = new EstimateServiceItem(Guid.NewGuid(), this, service, quantity);
        ServiceItems.Add(item);
        RecalculateTotal();
    }

    public void UpdateServiceItem(Service service, int quantity)
    {
        EnsureEditable();

        var item = ServiceItems.FirstOrDefault(x => x.ServiceId == service.Id)
            ?? throw new InvalidOperationException("Service item not found.");

        item.UpdateDetails(service, quantity);
        RecalculateTotal();
    }

    public void RemoveServiceItem(Guid itemId)
    {
        EnsureEditable();

        var item = ServiceItems.FirstOrDefault(x => x.Id == itemId)
            ?? throw new InvalidOperationException("Service item not found.");

        ServiceItems.Remove(item);
        RecalculateTotal();
    }

    public void AddPartItem(Product product, int quantity)
    {
        EnsureEditable();

        var item = new EstimateProductItem(Guid.NewGuid(), this, product, quantity);
        PartItems.Add(item);
        RecalculateTotal();
    }

    public void UpdatePartItem(Product product, int quantity)
    {
        EnsureEditable();

        var item = PartItems.FirstOrDefault(x => x.ProductId == product.Id)
            ?? throw new InvalidOperationException("Part item not found.");

        item.UpdateDetails(product, quantity);
        RecalculateTotal();
    }

    public void RemovePartItem(Guid itemId)
    {
        EnsureEditable();

        var item = PartItems.FirstOrDefault(x => x.Id == itemId)
            ?? throw new InvalidOperationException("Part item not found.");

        PartItems.Remove(item);
        RecalculateTotal();
    }

    public void SendToCustomer()
    {
        if (Status != EstimateStatus.Draft)
        {
            throw new InvalidOperationException("Only draft estimates can be sent to customer.");
        }

        Status = EstimateStatus.PendingApproval;
        SentAt = DateTime.UtcNow;
    }

    public void Approve()
    {
        if (Status != EstimateStatus.PendingApproval)
        {
            throw new InvalidOperationException("Only pending estimates can be approved.");
        }

        Status = EstimateStatus.Approved;
        ApprovedAt = DateTime.UtcNow;
    }

    public void Reject(string reason)
    {
        if (Status != EstimateStatus.PendingApproval)
        {
            throw new InvalidOperationException("Only pending estimates can be rejected.");
        }

        if (string.IsNullOrWhiteSpace(reason))
        {
            throw new ArgumentException("Rejection reason is required.", nameof(reason));
        }

        Status = EstimateStatus.Rejected;
        RejectedAt = DateTime.UtcNow;
        RejectionReason = reason;
    }

    private void EnsureEditable()
    {
        if (Status is EstimateStatus.Approved)
        {
            throw new InvalidOperationException("Estimate can only be edited before approval.");
        }
    }

    private void RecalculateTotal()
    {
        TotalAmount = ServiceItems.Sum(x => x.TotalPrice) + PartItems.Sum(x => x.TotalPrice);
    }

    public void RecalculateTotals()
    {
        RecalculateTotal();
    }
}
