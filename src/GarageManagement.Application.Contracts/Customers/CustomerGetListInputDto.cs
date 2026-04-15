using System.ComponentModel.DataAnnotations;
using Volo.Abp.Application.Dtos;

namespace GarageManagement.Customers
{
    public class CustomerGetListInputDto : PagedAndSortedResultRequestDto
    {
        [StringLength(256)]
        public string? Filter { get; set; }

        [StringLength(256)]
        public string? Name { get; set; }

        [StringLength(256)]
        public string? Email { get; set; }

        [StringLength(14)]
        public string? Document { get; set; }
    }
}