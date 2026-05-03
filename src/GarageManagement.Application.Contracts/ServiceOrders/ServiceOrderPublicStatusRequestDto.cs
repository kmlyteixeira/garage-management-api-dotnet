using System.ComponentModel.DataAnnotations;
using System.Text.RegularExpressions;

namespace GarageManagement.ServiceOrders;

public class ServiceOrderPublicStatusRequestDto
{
    [Required]
    [MaxLength(20)]
    public string Document { get; set; } = string.Empty;

    [Required]
    [MaxLength(20)]
    public string LicensePlate { get; set; } = string.Empty;

    public void Normalize()
    {
        Document = Regex.Replace(Document, "[^0-9a-zA-Z]", string.Empty);
        LicensePlate = Regex.Replace(LicensePlate, "[^0-9a-zA-Z]", string.Empty).ToUpperInvariant();
    }
}