using System.IO.Compression;
using System.Text.Json;
using Microsoft.EntityFrameworkCore;
using web_analizador_de_plagios.Data.Models;

namespace web_analizador_de_plagios.Services
{
    public class PaqueteRespaldoGrupo
    {
        public Grupo Grupo { get; set; } = new();
        public List<Estudiante> Estudiantes { get; set; } = new();
        public List<HistorialAnalisis> Historiales { get; set; } = new();
        public List<Evaluacion> Evaluaciones { get; set; } = new();
        public DateTime FechaExportacion { get; set; } = DateTime.UtcNow;
        public string VersionApp { get; set; } = "1.0.0";
    }

    public class GestorRespaldos
    {
        private readonly IDbContextFactory<AppDbContext> _contextFactory;
        private static readonly JsonSerializerOptions JsonOptions = new()
        {
            WriteIndented = true,
            ReferenceHandler = System.Text.Json.Serialization.ReferenceHandler.IgnoreCycles
        };

        public GestorRespaldos(IDbContextFactory<AppDbContext> contextFactory)
        {
            _contextFactory = contextFactory;
        }

        /// <summary>
        /// Exporta un grupo completo y sus historiales a un archivo comprimido .zip que contiene un JSON interno.
        /// </summary>
        public async Task<byte[]> ExportarGrupoZipAsync(int idGrupo)
        {
            await using var db = await _contextFactory.CreateDbContextAsync();

            var grupo = await db.grupos
                .AsNoTracking()
                .FirstOrDefaultAsync(g => g.IdGrupo == idGrupo);

            if (grupo == null)
            {
                throw new InvalidOperationException($"El grupo con ID {idGrupo} no fue encontrado.");
            }

            var estudiantes = await db.Estudiantes
                .AsNoTracking()
                .Where(e => e.IdGrupo == idGrupo)
                .ToListAsync();

            var idsEstudiantes = estudiantes.Select(e => e.IdEstudiante).ToList();

            var historiales = await db.Evaluaciones
                .AsNoTracking()
                .Where(h => idsEstudiantes.Contains(h.IdEstudiante))
                .ToListAsync();

            var idsEvaluaciones = historiales.Select(h => h.IdEvaluacion).Distinct().ToList();

            var evaluaciones = await db.Evaluacion
                .AsNoTracking()
                .Where(ev => idsEvaluaciones.Contains(ev.IdEvaluacion))
                .ToListAsync();

            var paquete = new PaqueteRespaldoGrupo
            {
                Grupo = grupo,
                Estudiantes = estudiantes,
                Historiales = historiales,
                Evaluaciones = evaluaciones,
                FechaExportacion = DateTime.UtcNow
            };

            string jsonContent = JsonSerializer.Serialize(paquete, JsonOptions);

            using var memoryStream = new MemoryStream();
            using (var archive = new ZipArchive(memoryStream, ZipArchiveMode.Create, true))
            {
                var entry = archive.CreateEntry($"respaldo_grupo_{grupo.IdGrupo}_{grupo.NombreGrupo.Replace(" ", "_")}.json", CompressionLevel.Optimal);
                await using var entryStream = entry.Open();
                await using var writer = new StreamWriter(entryStream, System.Text.Encoding.UTF8);
                await writer.WriteAsync(jsonContent);
            }

            return memoryStream.ToArray();
        }

        /// <summary>
        /// Importa un grupo completo desde un archivo comprimido .zip que contiene el JSON interno.
        /// </summary>
        public async Task<string> ImportarGrupoZipAsync(Stream zipStream)
        {
            using var archive = new ZipArchive(zipStream, ZipArchiveMode.Read);
            var jsonEntry = archive.Entries.FirstOrDefault(e => e.FullName.EndsWith(".json", StringComparison.OrdinalIgnoreCase));

            if (jsonEntry == null)
            {
                throw new FormatException("El archivo comprimido ZIP no contiene ningún archivo de respaldo .json válido.");
            }

            await using var entryStream = jsonEntry.Open();
            using var reader = new StreamReader(entryStream, System.Text.Encoding.UTF8);
            string jsonString = await reader.ReadToEndAsync();

            var paquete = JsonSerializer.Deserialize<PaqueteRespaldoGrupo>(jsonString, JsonOptions);
            if (paquete == null || paquete.Grupo == null)
            {
                throw new InvalidDataException("No se pudo deserializar el contenido del respaldo JSON.");
            }

            await using var db = await _contextFactory.CreateDbContextAsync();

            // Insertar o recrear grupo asegurando clave única de carnet por grupo
            var nuevoGrupo = new Grupo
            {
                NombreGrupo = paquete.Grupo.NombreGrupo + " (Importado " + DateTime.Now.ToString("dd/MM/yyyy HH:mm") + ")",
                AnioLectivo = paquete.Grupo.AnioLectivo,
                ciclo = paquete.Grupo.ciclo
            };

            db.grupos.Add(nuevoGrupo);
            await db.SaveChangesAsync();

            // Mapeo de evaluaciones para no duplicar si ya existen
            var mapaEvaluaciones = new Dictionary<int, int>();
            foreach (var ev in paquete.Evaluaciones)
            {
                var evExistente = await db.Evaluacion.FirstOrDefaultAsync(e => e.NombreTarea == ev.NombreTarea && e.Computo == ev.Computo);
                if (evExistente != null)
                {
                    mapaEvaluaciones[ev.IdEvaluacion] = evExistente.IdEvaluacion;
                }
                else
                {
                    var nuevaEv = new Evaluacion
                    {
                        NombreTarea = ev.NombreTarea,
                        Computo = ev.Computo,
                        FechaEvaluacion = ev.FechaEvaluacion
                    };
                    db.Evaluacion.Add(nuevaEv);
                    await db.SaveChangesAsync();
                    mapaEvaluaciones[ev.IdEvaluacion] = nuevaEv.IdEvaluacion;
                }
            }

            // Mapeo e importación de estudiantes
            var mapaEstudiantes = new Dictionary<int, int>();
            foreach (var est in paquete.Estudiantes)
            {
                var nuevoEst = new Estudiante
                {
                    Carnet = est.Carnet,
                    NombreCompleto = est.NombreCompleto,
                    IdGrupo = nuevoGrupo.IdGrupo
                };
                db.Estudiantes.Add(nuevoEst);
                await db.SaveChangesAsync();
                mapaEstudiantes[est.IdEstudiante] = nuevoEst.IdEstudiante;
            }

            // Importar historiales
            foreach (var hist in paquete.Historiales)
            {
                if (mapaEstudiantes.TryGetValue(hist.IdEstudiante, out int nuevoIdEstudiante))
                {
                    int nuevoIdEv = mapaEvaluaciones.ContainsKey(hist.IdEvaluacion) ? mapaEvaluaciones[hist.IdEvaluacion] : 0;
                    if (nuevoIdEv == 0)
                    {
                        var defaultEv = await db.Evaluacion.FirstOrDefaultAsync();
                        if (defaultEv == null)
                        {
                            defaultEv = new Evaluacion { NombreTarea = "Evaluación Importada", Computo = 1 };
                            db.Evaluacion.Add(defaultEv);
                            await db.SaveChangesAsync();
                        }
                        nuevoIdEv = defaultEv.IdEvaluacion;
                    }

                    var nuevoHist = new HistorialAnalisis
                    {
                        IdEstudiante = nuevoIdEstudiante,
                        IdEvaluacion = nuevoIdEv,
                        PorcentajePlagio = hist.PorcentajePlagio,
                        PorcentajeIA = hist.PorcentajeIA,
                        RutaArchivo = hist.RutaArchivo,
                        MetadatosJson = hist.MetadatosJson
                    };
                    db.Evaluaciones.Add(nuevoHist);
                }
            }

            await db.SaveChangesAsync();
            return $"Grupo '{nuevoGrupo.NombreGrupo}' importado con éxito: {mapaEstudiantes.Count} estudiantes y sus historiales restaurados.";
        }
    }
}
