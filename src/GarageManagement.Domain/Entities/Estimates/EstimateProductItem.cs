using System;
using GarageManagement.Products;
using Volo.Abp.Domain.Entities;

namespace GarageManagement.Estimates;

public class EstimateProductItem : Entity<Guid>
{
    public Guid EstimateId { get; private set; }
    public Guid ProductId { get; private set; }
    public int Quantity { get; private set; }
    public decimal TotalPrice { get; private set; }

    private EstimateProductItem()
    {
    }

    public EstimateProductItem(Guid id, Estimate estimate, Product product, int quantity)
        : base(id)
    {

        if (quantity <= 0)
        {
            throw new ArgumentException("Quantity must be greater than zero.", nameof(quantity));
        }

        EstimateId = estimate.Id;
        ProductId = product.Id;
        Quantity = quantity;
        TotalPrice = product.Price * quantity;
    }

    public void UpdateDetails(Product product, int quantity)
    {
        if (quantity <= 0)
        {
            throw new ArgumentException("Quantity must be greater than zero.", nameof(quantity));
        }

        ProductId = product.Id;
        Quantity = quantity;
        TotalPrice = product.Price * quantity;
    }
}
