using System;
using System.ComponentModel.DataAnnotations;

namespace GarageManagement.Inventories
{
    public class InventoryCreateUpdateDto
    {
        [Required(ErrorMessage = "O ID do produto é obrigatório")]
        public Guid ProductId { get; set; }

        [Required(ErrorMessage = "A quantidade é obrigatória")]
        [Range(0, int.MaxValue, ErrorMessage = "A quantidade não pode ser negativa")]
        public int Quantity { get; set; }
    }
}