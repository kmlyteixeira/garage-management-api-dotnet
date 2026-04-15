using System;
using GarageManagement.Products;
using Volo.Abp.Domain.Entities;

namespace GarageManagement.Inventories
{
    public class Inventory : Entity<Guid>
    {
        public Guid ProductId { get; set; }
        public virtual Product Product { get; set; }
        public int Quantity { get; set; }

        private Inventory()
        {
        }

        public Inventory(Product product, int quantity)
        {
            ProductId = product.Id;
            Product = product;
            Quantity = quantity;
        }

        public void AddStock(int amount)
        {
            if (amount < 0)
                throw new ArgumentException("Amount to add cannot be negative.");

            Quantity += amount;
        }

        public void DecreaseStock(int amount)
        {
            if (amount < 0)
                throw new ArgumentException("Amount to decrease cannot be negative.");

            if (amount > Quantity)
                throw new InvalidOperationException("Insufficient stock.");

            Quantity -= amount;
        }
    }
}