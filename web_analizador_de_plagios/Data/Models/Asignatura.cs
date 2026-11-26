using System.ComponentModel.DataAnnotations;

namespace web_analizador_de_plagios.Data.Models
{
    public class Asignatura
    {
        public int IdAsignatura { get; set; }

        [Required(ErrorMessage = "El código de asignatura es obligatorio.")]
        [StringLength(20, MinimumLength = 2, ErrorMessage = "El código debe tener entre 2 y 20 caracteres.")]
        public string Codigo { get; set; } = string.Empty;

        [Required(ErrorMessage = "El nombre de la asignatura es obligatorio.")]
        [StringLength(100, MinimumLength = 3, ErrorMessage = "El nombre debe tener entre 3 y 100 caracteres.")]
        public string Nombre { get; set; } = string.Empty;

        [Range(1, 10, ErrorMessage = "Las unidades valorativas deben estar entre 1 y 10.")]
        public int UnidadesValorativas { get; set; } = 4;
    }
}
