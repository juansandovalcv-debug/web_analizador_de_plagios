using System.Text.RegularExpressions;

namespace web_analizador_de_plagios.Services
{
    public class PreguntaDefensa
    {
        public int Numero { get; set; }
        public string Categoria { get; set; } = string.Empty;
        public string Pregunta { get; set; } = string.Empty;
        public string JustificacionTecnica { get; set; } = string.Empty;
        public string FragmentoCodigo { get; set; } = string.Empty;
    }

    public class FichaQuizDefensa
    {
        public string NombreEstudiante { get; set; } = string.Empty;
        public string Carnet { get; set; } = string.Empty;
        public string NombreArchivo { get; set; } = string.Empty;
        public DateTime FechaGeneracion { get; set; } = DateTime.Now;
        public List<PreguntaDefensa> Preguntas { get; set; } = new();
    }

    public class GeneradorQuizDefensa
    {
        public FichaQuizDefensa GenerarPreguntas(
            string codigoFuente,
            string nombreEstudiante,
            string carnet,
            string nombreArchivo,
            List<string>? patronesIADetectados = null)
        {
            var ficha = new FichaQuizDefensa
            {
                NombreEstudiante = nombreEstudiante,
                Carnet = carnet,
                NombreArchivo = nombreArchivo,
                FechaGeneracion = DateTime.Now
            };

            var listaPreguntas = new List<PreguntaDefensa>();
            int contador = 1;

            if (string.IsNullOrWhiteSpace(codigoFuente))
            {
                listaPreguntas.Add(new PreguntaDefensa
                {
                    Numero = contador++,
                    Categoria = "General",
                    Pregunta = "¿Cuál es el objetivo principal del programa y cómo estructuraste la solución?",
                    JustificacionTecnica = "Código vacío o no disponible."
                });
                ficha.Preguntas = listaPreguntas;
                return ficha;
            }

            // 1. Detección de bucles (for, foreach, while, do-while)
            var matchBucle = Regex.Match(codigoFuente, @"(for\s*\([^)]+\)|foreach\s*\([^)]+\)|while\s*\([^)]+\))");
            if (matchBucle.Success)
            {
                string bucleSnippet = matchBucle.Value;
                listaPreguntas.Add(new PreguntaDefensa
                {
                    Numero = contador++,
                    Categoria = "Control de Flujo e Iteración",
                    Pregunta = $"En la estructura '{bucleSnippet}', explica detalladamente la condición de parada o el criterio de iteración, y qué sucedería si los datos de entrada están vacíos o fuera de rango.",
                    JustificacionTecnica = "Verificar comprensión real de la lógica iterativa y condiciones límite.",
                    FragmentoCodigo = bucleSnippet
                });
            }

            // 2. Detección de métodos / funciones personalizadas
            var matchMetodo = Regex.Matches(codigoFuente, @"(?:public|private|protected|internal|static)\s+(?:async\s+)?([A-Za-z0-9_<>]+)\s+([A-Za-z0-9_]+)\s*\(([^)]*)\)");
            foreach (Match m in matchMetodo)
            {
                string tipoRetorno = m.Groups[1].Value;
                string nombreMetodo = m.Groups[2].Value;
                string parametros = m.Groups[3].Value;

                if (nombreMetodo.Equals("Main", StringComparison.OrdinalIgnoreCase)) continue;

                listaPreguntas.Add(new PreguntaDefensa
                {
                    Numero = contador++,
                    Categoria = "Diseño Modular y Métodos",
                    Pregunta = $"Explica el rol del método '{nombreMetodo}({parametros})'. ¿Por qué decidiste devolver el tipo '{tipoRetorno}' y cuáles son los efectos secundarios (side-effects) que genera?",
                    JustificacionTecnica = "Comprobar autoría en la división de responsabilidades y contratos de parámetros.",
                    FragmentoCodigo = m.Value
                });
                break; // Tomamos el método más representativo
            }

            // 3. Detección de estructuras de datos o colecciones (List, Dictionary, Array)
            var matchColeccion = Regex.Match(codigoFuente, @"\b(List<[^>]+>|Dictionary<[^>]+>|[A-Za-z0-9_]+\[\])\s+([A-Za-z0-9_]+)");
            if (matchColeccion.Success)
            {
                string tipoCol = matchColeccion.Groups[1].Value;
                string varCol = matchColeccion.Groups[2].Value;
                listaPreguntas.Add(new PreguntaDefensa
                {
                    Numero = contador++,
                    Categoria = "Estructuras de Datos & Complejidad",
                    Pregunta = $"Definiste la colección '{varCol}' como '{tipoCol}'. ¿Por qué elegiste esta estructura de datos frente a alternativas como un array nativo u otra colección? ¿Cuál es su costo de búsqueda o inserción?",
                    JustificacionTecnica = "Evaluar criterio algorítmico y justificación de tipos de memoria.",
                    FragmentoCodigo = matchColeccion.Value
                });
            }

            // 4. Pregunta orientada a patrones de IA o construcciones avanzadas si existen
            if (patronesIADetectados != null && patronesIADetectados.Count > 0)
            {
                string primerPatron = patronesIADetectados.First();
                listaPreguntas.Add(new PreguntaDefensa
                {
                    Numero = contador++,
                    Categoria = "Defensa de Patrones Detectados (Auditoría IA)",
                    Pregunta = $"El sistema detectó: '{primerPatron}'. Justifica técnicamente cómo concebiste esta implementación específica sin apoyarte en un asistente externo, explicando cada instrucción involucrada.",
                    JustificacionTecnica = "Contrastar posibles códigos generados por LLM con la explicación espontánea del alumno.",
                    FragmentoCodigo = primerPatron
                });
            }
            else
            {
                // Si no hay alerta de IA, evaluar manejo de excepciones o condicionales
                var matchCatch = Regex.Match(codigoFuente, @"catch\s*\(([^)]+)\)");
                if (matchCatch.Success)
                {
                    listaPreguntas.Add(new PreguntaDefensa
                    {
                        Numero = contador++,
                        Categoria = "Manejo de Errores y Robustez",
                        Pregunta = $"En el bloque '{matchCatch.Value}', ¿qué tipo específico de anomalía preveías capturar y cómo se recupera el sistema de esa falla?",
                        JustificacionTecnica = "Verificar diseño defensivo del código.",
                        FragmentoCodigo = matchCatch.Value
                    });
                }
                else
                {
                    listaPreguntas.Add(new PreguntaDefensa
                    {
                        Numero = contador++,
                        Categoria = "Casos Límite y Edge Cases",
                        Pregunta = "¿Qué posibles excepciones en tiempo de ejecución (runtime) podrían ocurrir en este algoritmo si el usuario introduce datos nulos o malformados, y cómo lo solventarías?",
                        JustificacionTecnica = "Evaluar comprensión de robustez en tiempo de ejecución."
                    });
                }
            }

            // 5. Pregunta de optimización / refactorización
            if (contador <= 5)
            {
                listaPreguntas.Add(new PreguntaDefensa
                {
                    Numero = contador++,
                    Categoria = "Evolución y Optimización",
                    Pregunta = "Si el volumen de datos o la frecuencia de ejecución de este algoritmo se multiplicara por 10,000 veces, ¿qué parte específica del código se convertiría en un cuello de botella y cómo lo optimizarías?",
                    JustificacionTecnica = "Comprobar capacidad de abstracción y análisis de complejidad temporal/espacial."
                });
            }

            ficha.Preguntas = listaPreguntas.Take(5).ToList();
            return ficha;
        }

        /// <summary>
        /// Genera el texto descargable formateado de la ficha de defensa oral.
        /// </summary>
        public string GenerarTextoFicha(FichaQuizDefensa ficha)
        {
            var sb = new System.Text.StringBuilder();
            sb.AppendLine("================================================================================");
            sb.AppendLine("              FICHA DE DEFENSA ORAL Y CUESTIONARIO ANTI-FRAUDE");
            sb.AppendLine("================================================================================");
            sb.AppendLine($"Estudiante: {ficha.NombreEstudiante}");
            sb.AppendLine($"Carnet:     {ficha.Carnet}");
            sb.AppendLine($"Archivo:    {ficha.NombreArchivo}");
            sb.AppendLine($"Fecha:      {ficha.FechaGeneracion:yyyy-MM-dd HH:mm:ss}");
            sb.AppendLine("================================================================================");
            sb.AppendLine();
            sb.AppendLine("INSTRUCCIONES PARA EL DOCENTE:");
            sb.AppendLine("Formule estas preguntas directamente al estudiante de forma oral. Solicite que señale");
            sb.AppendLine("y explique el código en pantalla sin consultar notas externas.");
            sb.AppendLine();

            foreach (var q in ficha.Preguntas)
            {
                sb.AppendLine($"[PREGUNTA #{q.Numero}] - Categoría: {q.Categoria}");
                sb.AppendLine($"Pregunta: {q.Pregunta}");
                if (!string.IsNullOrWhiteSpace(q.FragmentoCodigo))
                {
                    sb.AppendLine($"Fragmento de Referencia: {q.FragmentoCodigo}");
                }
                sb.AppendLine($"Criterio de Evaluación: {q.JustificacionTecnica}");
                sb.AppendLine("--------------------------------------------------------------------------------");
                sb.AppendLine("Calificación Docente: [  ] Excelente (10)  [  ] Regular (5)  [  ] Insuficiente (0)");
                sb.AppendLine("Observaciones:");
                sb.AppendLine();
                sb.AppendLine();
            }

            sb.AppendLine("================================================================================");
            sb.AppendLine("Firma del Docente Evaluador: __________________________________________________");

            return sb.ToString();
        }
    }
}
