using System;
using System.ComponentModel.DataAnnotations;
using Volo.Abp.Application.Dtos;

namespace GarageManagement.Inventories
{
    public class InventoryGetListInputDto : PagedAndSortedResultRequestDto
    {
        public Guid? ProductId { get; set; }

        [Range(0, int.MaxValue)]
        public int? MaxQuantity { get; set; }

        [Range(0, int.MaxValue)]
        public int? MinQuantity { get; set; }
    }
}