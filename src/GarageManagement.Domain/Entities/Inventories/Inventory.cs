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
        public int ReservedQuantity { get; set; }

        private Inventory()
        {
        }

        public Inventory(Product product, int quantity)
        {
            ProductId = product.Id;
            Product = product;
            Quantity = quantity;
            ReservedQuantity = 0;
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

            if (amount > Quantity - ReservedQuantity)
                throw new InvalidOperationException("Insufficient stock.");

            Quantity -= amount;
        }

        public void ReserveStock(int amount)
        {
            if (amount <= 0)
                throw new ArgumentException("Amount to reserve must be greater than zero.");

            if (amount > Quantity - ReservedQuantity)
                throw new InvalidOperationException("Insufficient available stock for reservation.");

            ReservedQuantity += amount;
        }

        public void ReleaseReservedStock(int amount)
        {
            if (amount <= 0)
                throw new ArgumentException("Amount to release must be greater than zero.");

            if (amount > ReservedQuantity)
                throw new InvalidOperationException("Insufficient reserved stock to release.");

            ReservedQuantity -= amount;
        }

        public void ConsumeReservedStock(int amount)
        {
            if (amount <= 0)
                throw new ArgumentException("Amount to consume must be greater than zero.");

            if (amount > ReservedQuantity)
                throw new InvalidOperationException("Insufficient reserved stock to consume.");

            ReservedQuantity -= amount;
            Quantity -= amount;
        }
    }
}