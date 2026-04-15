using System.ComponentModel.DataAnnotations;

namespace GarageManagement.Customers
{
    public class CustomerCreateUpdateDto
    {
        [Required(ErrorMessage = "O nome do cliente é obrigatório")]
        [StringLength(256, MinimumLength = 3, ErrorMessage = "O nome deve ter entre 3 e 256 caracteres")]
        public string Name { get; set; }

        [Required(ErrorMessage = "O email é obrigatório")]
        [EmailAddress(ErrorMessage = "Email inválido")]
        [StringLength(256, ErrorMessage = "O email não pode exceder 256 caracteres")]
        public string Email { get; set; }

        [Required(ErrorMessage = "O telefone é obrigatório")]
        [Phone(ErrorMessage = "Telefone inválido")]
        [StringLength(20, MinimumLength = 10, ErrorMessage = "O telefone deve ter entre 10 e 20 caracteres")]
        public string PhoneNumber { get; set; }

        [Required(ErrorMessage = "O documento é obrigatório")]
        [StringLength(14, MinimumLength = 11, ErrorMessage = "Documento deve ter 11 (CPF) ou 14 (CNPJ) caracteres")]
        public string Document { get; set; }
    }
}