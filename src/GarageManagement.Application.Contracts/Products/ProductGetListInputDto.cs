using System.ComponentModel.DataAnnotations;
using Volo.Abp.Application.Dtos;

namespace GarageManagement.Products
{
    public class ProductGetListInputDto : PagedAndSortedResultRequestDto
    {
        [StringLength(500)]
        public string? Description { get; set; }

        [Range(0, double.MaxValue)]
        public decimal? MinPrice { get; set; }

        [Range(0, double.MaxValue)]
        public decimal? MaxPrice { get; set; }
    }
}