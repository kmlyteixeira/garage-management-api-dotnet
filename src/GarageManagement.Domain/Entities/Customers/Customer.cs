using System;
using Volo.Abp.Domain.Entities;

namespace GarageManagement.Customers
{
    public class Customer : Entity<Guid>
    {
        public string Name { get; set; }
        public string Email { get; set; }
        public string PhoneNumber { get; set; }
        public Document Document { get; set; }

        public Customer(string name, string email, string phoneNumber, Document document)
        {
            Name = name;
            Email = email;
            PhoneNumber = phoneNumber;
            Document = document;
        }
    }
}