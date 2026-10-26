using System.Text.RegularExpressions;

namespace web_analizador_de_plagios.Services
{
    public class NormalizadorCodigo
    {
        // Expresión regular para comentarios simples (//...) y de bloque (/*...*/)
        private static readonly Regex BlockCommentsRegex = new Regex(@"/\*[\s\S]*?\*/", RegexOptions.Compiled);
        private static readonly Regex LineCommentsRegex = new Regex(@"//.*$", RegexOptions.Multiline | RegexOptions.Compiled);
        private static readonly Regex StringLiteralsRegex = new Regex(@"""(\\""|[^""])*""", RegexOptions.Compiled);
        private static readonly Regex MultipleSpacesRegex = new Regex(@"\s+", RegexOptions.Compiled);

        /// <summary>
        /// Remueve comentarios, cadenas de texto literales (para enfocarse en lógica pura),
        /// directivas using y normaliza espacios y saltos de línea.
        /// </summary>
        public string Normalizar(string codigoOriginal)
        {
            if (string.IsNullOrWhiteSpace(codigoOriginal))
                return string.Empty;

            // 1. Quitar comentarios de bloque y de línea
            string sinComentarios = BlockCommentsRegex.Replace(codigoOriginal, " ");
            sinComentarios = LineCommentsRegex.Replace(sinComentarios, " ");

            // 2. Normalizar strings literales a un token genérico "STR" para evitar engaños por nombres de texto
            string sinStrings = StringLiteralsRegex.Replace(sinComentarios, "\"__STR__\"");

            // 3. Quitar directivas using y namespaces boilerplate
            var lineas = sinStrings.Split(new[] { "\r\n", "\r", "\n" }, StringSplitOptions.None);
            var lineasFiltradas = new List<string>();

            foreach (var l in lineas)
            {
                var trimmed = l.Trim();
                if (trimmed.StartsWith("using ") || trimmed.StartsWith("#region") || trimmed.StartsWith("#endregion"))
                    continue;

                lineasFiltradas.Add(trimmed);
            }

            // 4. Reensamblar y normalizar espaciado redundante
            string resultado = string.Join(" ", lineasFiltradas);
            resultado = MultipleSpacesRegex.Replace(resultado, " ").Trim();

            return resultado;
        }

        /// <summary>
        /// Calcula la distancia de Levenshtein entre dos textos de código normalizado.
        /// </summary>
        public int CalcularDistanciaLevenshtein(string s, string t)
        {
            if (string.IsNullOrEmpty(s)) return t?.Length ?? 0;
            if (string.IsNullOrEmpty(t)) return s.Length;

            int n = s.Length;
            int m = t.Length;
            int[,] d = new int[n + 1, m + 1];

            for (int i = 0; i <= n; d[i, 0] = i++) { }
            for (int j = 0; j <= m; d[0, j] = j++) { }

            for (int i = 1; i <= n; i++)
            {
                for (int j = 1; j <= m; j++)
                {
                    int cost = (t[j - 1] == s[i - 1]) ? 0 : 1;
                    d[i, j] = Math.Min(
                        Math.Min(d[i - 1, j] + 1, d[i, j - 1] + 1),
                        d[i - 1, j - 1] + cost);
                }
            }

            return d[n, m];
        }

        /// <summary>
        /// Calcula el porcentaje de similitud basado en la distancia de Levenshtein (0 a 100%).
        /// </summary>
        public double CalcularPorcentajeSimilitud(string codigoA, string codigoB)
        {
            string normA = Normalizar(codigoA);
            string normB = Normalizar(codigoB);

            if (string.IsNullOrEmpty(normA) && string.IsNullOrEmpty(normB)) return 100.0;
            if (string.IsNullOrEmpty(normA) || string.IsNullOrEmpty(normB)) return 0.0;

            if (normA == normB) return 100.0;

            int maxLen = Math.Max(normA.Length, normB.Length);
            // Optimización si las cadenas son masivamente largas (> 3000 caracteres)
            if (maxLen > 3000)
            {
                // Usamos n-gram similarity (Jaccard sobre 3-gramas) para rendimiento O(N)
                return CalcularSimilitudNGrams(normA, normB, 3) * 100.0;
            }

            int distancia = CalcularDistanciaLevenshtein(normA, normB);
            double similitud = (1.0 - ((double)distancia / maxLen)) * 100.0;
            return Math.Round(Math.Clamp(similitud, 0.0, 100.0), 2);
        }

        private double CalcularSimilitudNGrams(string s1, string s2, int n)
        {
            if (s1.Length < n || s2.Length < n) return s1 == s2 ? 1.0 : 0.0;

            var set1 = new HashSet<string>();
            for (int i = 0; i <= s1.Length - n; i++)
                set1.Add(s1.Substring(i, n));

            var set2 = new HashSet<string>();
            for (int i = 0; i <= s2.Length - n; i++)
                set2.Add(s2.Substring(i, n));

            int interseccion = 0;
            foreach (var item in set1)
            {
                if (set2.Contains(item))
                    interseccion++;
            }

            int union = set1.Count + set2.Count - interseccion;
            return union == 0 ? 0.0 : (double)interseccion / union;
        }
    }
}
