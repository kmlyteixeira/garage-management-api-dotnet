using System;
using GarageManagement.Services;
using Volo.Abp.Domain.Entities;

namespace GarageManagement.Estimates;

public class EstimateServiceItem : Entity<Guid>
{
    public Guid EstimateId { get; private set; }
    public Guid ServiceId { get; private set; }
    public int Quantity { get; private set; }
    public decimal TotalPrice { get; private set; }

    private EstimateServiceItem()
    {
    }

    public EstimateServiceItem(Guid id, Estimate estimate, Service service, int quantity)
        : base(id)
    {
        if (quantity <= 0)
        {
            throw new ArgumentException("Quantity must be greater than zero.", nameof(quantity));
        }

        EstimateId = estimate.Id;
        ServiceId = service.Id;
        Quantity = quantity;
        TotalPrice = service.Price * quantity;
    }

    public void UpdateDetails(Service service, int quantity)
    {
        if (quantity <= 0)
        {
            throw new ArgumentException("Quantity must be greater than zero.", nameof(quantity));
        }

        ServiceId = service.Id;
        Quantity = quantity;
        TotalPrice = service.Price * quantity;
    }
}
