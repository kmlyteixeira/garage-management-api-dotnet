using System.ComponentModel.DataAnnotations;
using Volo.Abp.Application.Dtos;

namespace GarageManagement.Vehicles
{
    public class VehicleGetListInputDto : PagedAndSortedResultRequestDto
    {
        [StringLength(100)]
        public string? Make { get; set; }

        [StringLength(100)]
        public string? Model { get; set; }

        [Range(1900, 2100)]
        public int? Year { get; set; }

        [StringLength(10)]
        public string? LicensePlate { get; set; }
    }
}