using System;
using System.ComponentModel.DataAnnotations;
using Volo.Abp.Application.Dtos;

namespace GarageManagement.Inventories
{
    public class InventoryDto : EntityDto<Guid>
    {
        [Required(ErrorMessage = "O ID do produto é obrigatório")]
        public Guid ProductId { get; set; }

        [Required(ErrorMessage = "A descrição do produto é obrigatória")]
        public string ProductDescription { get; set; }

        [Required(ErrorMessage = "O preço do produto é obrigatório")]
        [Range(0.01, double.MaxValue, ErrorMessage = "O preço deve ser maior que zero")]
        public decimal ProductPrice { get; set; }

        [Required(ErrorMessage = "A quantidade é obrigatória")]
        [Range(0, int.MaxValue, ErrorMessage = "A quantidade não pode ser negativa")]
        public int Quantity { get; set; }
    }
}