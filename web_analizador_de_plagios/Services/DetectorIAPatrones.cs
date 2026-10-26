using System.Text.RegularExpressions;

namespace web_analizador_de_plagios.Services
{
    public class DetectorIAPatrones
    {
        // Patrones sintácticos comunes de generadores LLM (ChatGPT, Claude, Copilot) en C#
        private static readonly string[] MarcadoresExplicativos = new[]
        {
            "Here is the code",
            "Here's the code",
            "Aquí está el código",
            "Explicación del código",
            "This method handles",
            "Step 1:",
            "Step 2:",
            "Note:",
            "Nota:",
            "In summary",
            "En resumen",
            "As an AI",
            "Como modelo de lenguaje"
        };

        private static readonly string[] NombresGenericosVariables = new[]
        {
            "helper", "processData", "dummyData", "exampleValue",
            "tempResult", "inputString", "targetValue", "itemContainer"
        };

        public class ResultadoDeteccionIA
        {
            public float ProbabilidadIA { get; set; }
            public List<string> PatronesDetectados { get; set; } = new();
            public string NivelRiesgo { get; set; } = "Bajo";
        }

        public ResultadoDeteccionIA Analizar(string codigoFuente)
        {
            var resultado = new ResultadoDeteccionIA();
            if (string.IsNullOrWhiteSpace(codigoFuente))
            {
                return resultado;
            }

            int puntaje = 0;

            // 1. Detección de comentarios en inglés explicativo / marcadores de IA
            foreach (var marcador in MarcadoresExplicativos)
            {
                if (codigoFuente.Contains(marcador, StringComparison.OrdinalIgnoreCase))
                {
                    puntaje += 40;
                    resultado.PatronesDetectados.Add($"Comentario o encabezado típico de LLM detectado: '{marcador}'");
                }
            }

            // 2. Proporción exagerada de comentarios XML de documentación /// <summary>
            int conteoSummary = Regex.Matches(codigoFuente, @"///\s*<summary>", RegexOptions.IgnoreCase).Count;
            int totalLineas = codigoFuente.Split('\n').Length;

            if (conteoSummary >= 3 && totalLineas < 120)
            {
                puntaje += 25;
                resultado.PatronesDetectados.Add($"Sobrecarga inusual de documentación XML (/// <summary>) para una tarea breve ({conteoSummary} bloques).");
            }

            // 3. Formato excesivamente perfecto y comentarios pedagógicos
            int comentariosPedagogicos = Regex.Matches(codigoFuente, @"//\s*(Paso\s*\d|Ensure|Check if|Validate that|Initialize|Handle potential|Loop through)", RegexOptions.IgnoreCase).Count;
            if (comentariosPedagogicos >= 3)
            {
                puntaje += 20;
                resultado.PatronesDetectados.Add($"Comentarios con estilo de instrucción pedagógica generada por IA ({comentariosPedagogicos} detectados).");
            }

            // 4. Variables excesivamente canónicas o genéricas de prompts de LLM
            int variablesGenericasDetectadas = 0;
            foreach (var varName in NombresGenericosVariables)
            {
                if (Regex.IsMatch(codigoFuente, $@"\b{varName}\b", RegexOptions.IgnoreCase))
                {
                    variablesGenericasDetectadas++;
                }
            }

            if (variablesGenericasDetectadas >= 2)
            {
                puntaje += 15;
                resultado.PatronesDetectados.Add($"Nomenclatura estándar de IA detectada ({variablesGenericasDetectadas} identificadores típicos).");
            }

            // 5. Manejo hiper-defensivo de excepciones en ejercicios de nivel formativo
            int checksNullDefensivos = Regex.Matches(codigoFuente, @"ArgumentNullException\.ThrowIfNull|throw new ArgumentNullException", RegexOptions.IgnoreCase).Count;
            if (checksNullDefensivos >= 2 && totalLineas < 100)
            {
                puntaje += 15;
                resultado.PatronesDetectados.Add("Uso de validaciones hiper-defensivas de producción estándar de IA en ejercicios introductorios.");
            }

            // Normalizar a porcentaje (0 a 100)
            float probabilidadFinal = Math.Clamp(puntaje, 0, 100);
            resultado.ProbabilidadIA = probabilidadFinal;

            if (probabilidadFinal >= 70)
                resultado.NivelRiesgo = "Muy Alto";
            else if (probabilidadFinal >= 45)
                resultado.NivelRiesgo = "Medio";
            else if (probabilidadFinal >= 20)
                resultado.NivelRiesgo = "Bajo";
            else
                resultado.NivelRiesgo = "Mínimo";

            return resultado;
        }
    }
}
