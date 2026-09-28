using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace web_analizador_de_plagios.Migrations
{
    /// <inheritdoc />
    public partial class InitialCreate : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.CreateTable(
                name: "Evaluacion",
                columns: table => new
                {
                    IdEvaluacion = table.Column<int>(type: "INTEGER", nullable: false)
                        .Annotation("Sqlite:Autoincrement", true),
                    NombreTarea = table.Column<string>(type: "TEXT", nullable: false),
                    Computo = table.Column<int>(type: "INTEGER", nullable: false),
                    FechaEvaluacion = table.Column<DateTime>(type: "TEXT", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_Evaluacion", x => x.IdEvaluacion);
                });

            migrationBuilder.CreateTable(
                name: "grupos",
                columns: table => new
                {
                    IdGrupo = table.Column<int>(type: "INTEGER", nullable: false)
                        .Annotation("Sqlite:Autoincrement", true),
                    NombreGrupo = table.Column<string>(type: "TEXT", nullable: false),
                    AnioLectivo = table.Column<int>(type: "INTEGER", nullable: false),
                    ciclo = table.Column<int>(type: "INTEGER", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_grupos", x => x.IdGrupo);
                });

            migrationBuilder.CreateTable(
                name: "Estudiantes",
                columns: table => new
                {
                    IdEstudiante = table.Column<int>(type: "INTEGER", nullable: false)
                        .Annotation("Sqlite:Autoincrement", true),
                    Carnet = table.Column<string>(type: "TEXT", nullable: false),
                    NombreCompleto = table.Column<string>(type: "TEXT", nullable: false),
                    IdGrupo = table.Column<int>(type: "INTEGER", nullable: false),
                    GrupoIdGrupo = table.Column<int>(type: "INTEGER", nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_Estudiantes", x => x.IdEstudiante);
                    table.ForeignKey(
                        name: "FK_Estudiantes_grupos_GrupoIdGrupo",
                        column: x => x.GrupoIdGrupo,
                        principalTable: "grupos",
                        principalColumn: "IdGrupo");
                });

            migrationBuilder.CreateTable(
                name: "Evaluaciones",
                columns: table => new
                {
                    IdHistorial = table.Column<int>(type: "INTEGER", nullable: false)
                        .Annotation("Sqlite:Autoincrement", true),
                    PorcentajePlagio = table.Column<float>(type: "REAL", nullable: false),
                    PorcentajeIA = table.Column<float>(type: "REAL", nullable: false),
                    RutaArchivo = table.Column<string>(type: "TEXT", nullable: false),
                    MetadatosJson = table.Column<string>(type: "TEXT", nullable: false),
                    IdEstudiante = table.Column<int>(type: "INTEGER", nullable: false),
                    EstudianteIdEstudiante = table.Column<int>(type: "INTEGER", nullable: true),
                    IdEvaluacion = table.Column<int>(type: "INTEGER", nullable: false),
                    EvaluacionIdEvaluacion = table.Column<int>(type: "INTEGER", nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_Evaluaciones", x => x.IdHistorial);
                    table.ForeignKey(
                        name: "FK_Evaluaciones_Estudiantes_EstudianteIdEstudiante",
                        column: x => x.EstudianteIdEstudiante,
                        principalTable: "Estudiantes",
                        principalColumn: "IdEstudiante");
                    table.ForeignKey(
                        name: "FK_Evaluaciones_Evaluacion_EvaluacionIdEvaluacion",
                        column: x => x.EvaluacionIdEvaluacion,
                        principalTable: "Evaluacion",
                        principalColumn: "IdEvaluacion");
                });

            migrationBuilder.CreateIndex(
                name: "IX_Estudiantes_Carnet_IdGrupo",
                table: "Estudiantes",
                columns: new[] { "Carnet", "IdGrupo" },
                unique: true);

            migrationBuilder.CreateIndex(
                name: "IX_Estudiantes_GrupoIdGrupo",
                table: "Estudiantes",
                column: "GrupoIdGrupo");

            migrationBuilder.CreateIndex(
                name: "IX_Evaluaciones_EstudianteIdEstudiante",
                table: "Evaluaciones",
                column: "EstudianteIdEstudiante");

            migrationBuilder.CreateIndex(
                name: "IX_Evaluaciones_EvaluacionIdEvaluacion",
                table: "Evaluaciones",
                column: "EvaluacionIdEvaluacion");
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropTable(
                name: "Evaluaciones");

            migrationBuilder.DropTable(
                name: "Estudiantes");

            migrationBuilder.DropTable(
                name: "Evaluacion");

            migrationBuilder.DropTable(
                name: "grupos");
        }
    }
}
