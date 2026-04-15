using System;
using System.ComponentModel.DataAnnotations;
using Volo.Abp.Application.Dtos;

namespace GarageManagement.Customers
{
    public class CustomerDto : EntityDto<Guid>
    {
        [Required(ErrorMessage = "O nome do cliente é obrigatório")]
        [StringLength(256, ErrorMessage = "O nome não pode exceder 256 caracteres")]
        public string Name { get; set; }

        [Required(ErrorMessage = "O email é obrigatório")]
        [EmailAddress(ErrorMessage = "Email inválido")]
        public string Email { get; set; }

        [Required(ErrorMessage = "O telefone é obrigatório")]
        [Phone(ErrorMessage = "Telefone inválido")]
        public string PhoneNumber { get; set; }

        [Required(ErrorMessage = "O documento é obrigatório")]
        [StringLength(14, MinimumLength = 11, ErrorMessage = "Documento deve ter 11 ou 14 caracteres")]
        public string Document { get; set; }
    }
}