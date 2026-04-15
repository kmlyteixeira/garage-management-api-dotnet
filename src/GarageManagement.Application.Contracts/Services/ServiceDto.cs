using System;
using System.ComponentModel.DataAnnotations;
using Volo.Abp.Application.Dtos;

namespace GarageManagement.Services
{
    public class ServiceDto : EntityDto<Guid>
    {
        [Required(ErrorMessage = "A descrição do serviço é obrigatória")]
        [StringLength(500, MinimumLength = 3, ErrorMessage = "A descrição deve ter entre 3 e 500 caracteres")]
        public string Description { get; set; }

        [Required(ErrorMessage = "O preço é obrigatório")]
        [Range(0.01, double.MaxValue, ErrorMessage = "O preço deve ser maior que zero")]
        public decimal Price { get; set; }

        [Required(ErrorMessage = "O tempo estimado é obrigatório")]
        [Range(0.1, double.MaxValue, ErrorMessage = "O tempo estimado deve ser maior que 0 horas")]
        public double EstimatedTime { get; set; }
    }
}