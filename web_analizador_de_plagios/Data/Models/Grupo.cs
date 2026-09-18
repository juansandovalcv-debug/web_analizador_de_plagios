using System.Security.Cryptography.X509Certificates;

namespace web_analizador_de_plagios.Data.Models
{
    public class Grupo
    {
        public int IdGrupo { get; set; }
        public string NombreGrupo { get; set; } = string.Empty;
        public int AnioLectivo { get; set; } = DateTime.Now.Year;
        public int ciclo { get; set; }


        public ICollection<Estudiante> Estudiantes { get; set; } = new List<Estudiante>();

    }
}

