using System.ComponentModel.DataAnnotations;

namespace GarageManagement.Vehicles
{
    public class VehicleCreateUpdateDto
    {
        [Required(ErrorMessage = "A marca do veículo é obrigatória")]
        [StringLength(100, MinimumLength = 2, ErrorMessage = "A marca deve ter entre 2 e 100 caracteres")]
        public string Make { get; set; }

        [Required(ErrorMessage = "O modelo do veículo é obrigatório")]
        [StringLength(100, MinimumLength = 2, ErrorMessage = "O modelo deve ter entre 2 e 100 caracteres")]
        public string Model { get; set; }

        [Required(ErrorMessage = "O ano do veículo é obrigatório")]
        [Range(1900, 2100, ErrorMessage = "O ano deve estar entre 1900 e 2100")]
        public int Year { get; set; }

        [Required(ErrorMessage = "A placa do veículo é obrigatória")]
        [StringLength(10, MinimumLength = 7, ErrorMessage = "A placa deve ter entre 7 e 10 caracteres")]
        [RegularExpression(@"[A-Z]{3}-\d{4}|[A-Z]{3}\d[A-Z]\d{2}", ErrorMessage = "Formato de placa inválido")]
        public string LicensePlate { get; set; }
    }
}