using System;
using Volo.Abp.Domain.Entities;

namespace GarageManagement.Services
{
    public class Service : Entity<Guid>
    {
        public string Description { get; set; }
        public decimal Price { get; set; }

        /// <summary>
        /// Estimated time to complete the service in hours.
        /// </summary>
        public double EstimatedTime { get; set; }

        public Service(string description, decimal price, double estimatedTime)
        {
            Description = description;
            Price = price;
            EstimatedTime = estimatedTime;
        }
    }
}