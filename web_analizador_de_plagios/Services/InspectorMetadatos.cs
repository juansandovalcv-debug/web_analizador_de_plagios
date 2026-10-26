using System.Security.AccessControl;
using System.Security.Principal;
using System.Text;

namespace web_analizador_de_plagios.Services
{
    public class InformacionMetadatos
    {
        public string NombreArchivo { get; set; } = string.Empty;
        public long TamanoBytes { get; set; }
        public string TamanoFormateado { get; set; } = string.Empty;
        public DateTime? FechaCreacionUtc { get; set; }
        public DateTime? FechaModificacionUtc { get; set; }
        public DateTime? FechaUltimoAccesoUtc { get; set; }
        public string Propietario { get; set; } = "Desconocido / No disponible";
        public string CodificacionDetectada { get; set; } = "UTF-8";
        public int TotalLineas { get; set; }
        public int TotalCaracteres { get; set; }
        public bool ContieneBOM { get; set; }
        public List<string> Advertencias { get; set; } = new();
    }

    public class InspectorMetadatos
    {
        /// <summary>
        /// Inspecciona un archivo en disco local de forma segura extrayendo atributos NTFS / Windows OS.
        /// </summary>
        public async Task<InformacionMetadatos> InspeccionarArchivoAsync(string rutaCompletaArchivo)
        {
            var info = new InformacionMetadatos();

            if (string.IsNullOrWhiteSpace(rutaCompletaArchivo) || !File.Exists(rutaCompletaArchivo))
            {
                info.Advertencias.Add("El archivo especificado no existe o la ruta no es válida.");
                return info;
            }

            try
            {
                var fileInfo = new FileInfo(rutaCompletaArchivo);
                info.NombreArchivo = fileInfo.Name;
                info.TamanoBytes = fileInfo.Length;
                info.TamanoFormateado = FormatearTamano(fileInfo.Length);
                info.FechaCreacionUtc = fileInfo.CreationTimeUtc;
                info.FechaModificacionUtc = fileInfo.LastWriteTimeUtc;
                info.FechaUltimoAccesoUtc = fileInfo.LastAccessTimeUtc;

                // Extraer propietario de seguridad de Windows si está disponible
                if (OperatingSystem.IsWindows())
                {
                    try
                    {
                        var fileSecurity = new FileSecurity(rutaCompletaArchivo, AccessControlSections.Owner);
                        var identity = fileSecurity.GetOwner(typeof(NTAccount));
                        if (identity != null)
                        {
                            info.Propietario = identity.Value;
                        }
                    }
                    catch (Exception ex)
                    {
                        info.Propietario = $"No disponible ({ex.Message})";
                    }
                }

                // Detectar codificación y estadísticas de líneas
                byte[] bytes = await File.ReadAllBytesAsync(rutaCompletaArchivo);
                info.CodificacionDetectada = DetectarCodificacion(bytes, out bool tieneBOM);
                info.ContieneBOM = tieneBOM;

                string contenido = info.CodificacionDetectada.Contains("UTF-8")
                    ? Encoding.UTF8.GetString(bytes)
                    : Encoding.ASCII.GetString(bytes);

                info.TotalCaracteres = contenido.Length;
                info.TotalLineas = contenido.Split('\n').Length;

                // Análisis forense de discrepancia de fechas (ej. archivo creado después de ser modificado)
                if (fileInfo.LastWriteTimeUtc < fileInfo.CreationTimeUtc.AddMinutes(-5))
                {
                    info.Advertencias.Add("Alerta Forense: La fecha de última modificación es anterior a la fecha de creación en este sistema (posible archivo copiado o descargado de otra máquina).");
                }
            }
            catch (Exception ex)
            {
                info.Advertencias.Add($"Error al inspeccionar metadatos: {ex.Message}");
            }

            return info;
        }

        /// <summary>
        /// Inspecciona metadatos a partir de un flujo de bytes en memoria (cuando el archivo se sube desde el navegador).
        /// </summary>
        public InformacionMetadatos InspeccionarBytes(string nombreArchivo, byte[] bytes, DateTime? fechaNavegadorUtc = null)
        {
            var info = new InformacionMetadatos
            {
                NombreArchivo = Path.GetFileName(nombreArchivo),
                TamanoBytes = bytes.Length,
                TamanoFormateado = FormatearTamano(bytes.Length),
                FechaModificacionUtc = fechaNavegadorUtc ?? DateTime.UtcNow,
                FechaCreacionUtc = fechaNavegadorUtc ?? DateTime.UtcNow,
                Propietario = "Usuario de Sesión Web"
            };

            info.CodificacionDetectada = DetectarCodificacion(bytes, out bool tieneBOM);
            info.ContieneBOM = tieneBOM;

            string contenido = Encoding.UTF8.GetString(bytes);
            info.TotalCaracteres = contenido.Length;
            info.TotalLineas = contenido.Split('\n').Length;

            return info;
        }

        private static string DetectarCodificacion(byte[] bytes, out bool tieneBOM)
        {
            tieneBOM = false;
            if (bytes == null || bytes.Length == 0) return "UTF-8 (Vacío)";

            // Detectar UTF-8 BOM (0xEF, 0xBB, 0xBF)
            if (bytes.Length >= 3 && bytes[0] == 0xEF && bytes[1] == 0xBB && bytes[2] == 0xBF)
            {
                tieneBOM = true;
                return "UTF-8 con BOM";
            }

            // Detectar UTF-16 LE BOM (0xFF, 0xFE)
            if (bytes.Length >= 2 && bytes[0] == 0xFF && bytes[1] == 0xFE)
            {
                tieneBOM = true;
                return "UTF-16 LE";
            }

            // Verificar si es ASCII estricto (todos los bytes < 128)
            bool esPuroAscii = true;
            for (int i = 0; i < Math.Min(bytes.Length, 4096); i++)
            {
                if (bytes[i] >= 128)
                {
                    esPuroAscii = false;
                    break;
                }
            }

            if (esPuroAscii) return "ASCII (7-bit)";

            return "UTF-8 (Sin BOM)";
        }

        private static string FormatearTamano(long bytes)
        {
            if (bytes < 1024) return $"{bytes} B";
            if (bytes < 1024 * 1024) return $"{bytes / 1024.0:F2} KB";
            return $"{bytes / (1024.0 * 1024.0):F2} MB";
        }
    }
}
