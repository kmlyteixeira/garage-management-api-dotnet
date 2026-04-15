using System;
using Volo.Abp.Domain.Entities;

namespace GarageManagement.Products
{
    public class Product : Entity<Guid>
    {
        public string Description { get; set; }
        public decimal Price { get; set; }

        public Product(string description, decimal price)
        {
            Description = description;
            Price = price;
        }
    }
}