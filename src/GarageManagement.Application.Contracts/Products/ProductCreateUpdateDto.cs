using System.ComponentModel.DataAnnotations;

namespace GarageManagement.Products
{
    public class ProductCreateUpdateDto
    {
        [Required(ErrorMessage = "A descrição do produto é obrigatória")]
        [StringLength(500, MinimumLength = 3, ErrorMessage = "A descrição deve ter entre 3 e 500 caracteres")]
        public string Description { get; set; }

        [Required(ErrorMessage = "O preço é obrigatório")]
        [Range(0.01, double.MaxValue, ErrorMessage = "O preço deve ser maior que zero")]
        public decimal Price { get; set; }
    }
}