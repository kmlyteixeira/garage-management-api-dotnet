using System;
using Volo.Abp.Domain.Entities;

namespace GarageManagement.Vehicles
{
    public class Vehicle : Entity<Guid>
    {
        public string Make { get; set; }
        public string Model { get; set; }
        public int Year { get; set; }
        public string LicensePlate { get; set; }

        public Vehicle(string make, string model, int year, string licensePlate)
        {
            Make = make;
            Model = model;
            Year = year;
            LicensePlate = licensePlate;
        }
    }
}