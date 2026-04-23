using System;
using System.ComponentModel.DataAnnotations;
using Volo.Abp.Application.Dtos;

namespace GarageManagement.Inventories
{
    public class InventoryDto : EntityDto<Guid>
    {
        public Guid ProductId { get; set; }

        public int Quantity { get; set; }

        public int ReservedQuantity { get; set; }
    }
}