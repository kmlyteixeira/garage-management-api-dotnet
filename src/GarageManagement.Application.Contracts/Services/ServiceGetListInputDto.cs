using System.ComponentModel.DataAnnotations;
using Volo.Abp.Application.Dtos;

namespace GarageManagement.Services
{
    public class ServiceGetListInputDto : PagedAndSortedResultRequestDto
    {
        [StringLength(500)]
        public string? Description { get; set; }

        [Range(0, double.MaxValue)]
        public decimal? MinPrice { get; set; }

        [Range(0, double.MaxValue)]
        public decimal? MaxPrice { get; set; }

        [Range(0, double.MaxValue)]
        public double? MaxEstimatedTime { get; set; }
    }
}