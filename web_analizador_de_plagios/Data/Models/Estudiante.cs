namespace web_analizador_de_plagios.Data.Models
{
    public class Estudiante
    {
        public int IdEstudiante { get; set; }
        public string Carnet {  get; set; } = string.Empty;
        public string NombreCompleto { get; set; } = string.Empty;

        public int IdGrupo { get; set; }
        public Grupo? Grupo { get; set; }

        public ICollection<HistorialAnalisis> Historiales { get; set; } = new List<HistorialAnalisis>();
    }
}
