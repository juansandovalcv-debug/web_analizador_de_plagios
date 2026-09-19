namespace web_analizador_de_plagios.Data.Models
{
    public class Evaluacion
    {
        public int IdEvaluacion { get; set; }
        public string NombreTarea { get; set; } = string.Empty;
        public int Computo { get; set; }
        public DateTime FechaEvaluacion { get; set; } = DateTime.Now;
    }
}
