using System.ComponentModel.DataAnnotations;

namespace web_analizador_de_plagios.Data.Models
{
    public class Docente
    {
        public int IdDocente { get; set; }

        [Required(ErrorMessage = "El nombre completo es obligatorio.")]
        [StringLength(120, MinimumLength = 3, ErrorMessage = "El nombre debe tener entre 3 y 120 caracteres.")]
        public string NombreCompleto { get; set; } = string.Empty;

        [Required(ErrorMessage = "El correo institucional es obligatorio.")]
        [EmailAddress(ErrorMessage = "Ingrese un correo electrónico válido.")]
        [StringLength(120)]
        public string Correo { get; set; } = string.Empty;

        [Required(ErrorMessage = "La especialidad es obligatoria.")]
        [StringLength(80, MinimumLength = 2, ErrorMessage = "La especialidad debe tener entre 2 y 80 caracteres.")]
        public string Especialidad { get; set; } = string.Empty;

        public bool Activo { get; set; } = true;
    }
}
