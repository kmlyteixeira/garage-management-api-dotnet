using System.ComponentModel.DataAnnotations;

namespace GarageManagement.Customers;

public class CustomerSetActiveDto
{
    [Required]
    public bool IsActive { get; set; }
}