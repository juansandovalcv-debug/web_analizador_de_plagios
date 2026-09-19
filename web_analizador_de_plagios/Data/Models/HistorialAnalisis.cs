namespace web_analizador_de_plagios.Data.Models
{
    public class HistorialAnalisis
    {
        public int IdHistorial {  get; set; }
        public float PorcentajePlagio { get; set; }
        public float PorcentajeIA {  get; set; }
        public string RutaArchivo { get; set; } = string.Empty;
        public string MetadatosJson { get; set; } = "{}";

        public int IdEstudiante { get; set; }
        public Estudiante? estudiante { get; set; }

        // estas son las llaves foraneas que conectan la clase evaluacion para que se saque el porcentaje de plagio


        public int IdEvaluacion { get; set; }
        public Evaluacion? Evaluacion { get; set; }
    }
}
