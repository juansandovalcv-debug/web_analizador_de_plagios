using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace web_analizador_de_plagios.Migrations
{
    /// <inheritdoc />
    public partial class InitialSqlServerCreate : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.CreateTable(
                name: "Evaluacion",
                columns: table => new
                {
                    IdEvaluacion = table.Column<int>(type: "int", nullable: false)
                        .Annotation("SqlServer:Identity", "1, 1"),
                    NombreTarea = table.Column<string>(type: "nvarchar(max)", nullable: false),
                    Computo = table.Column<int>(type: "int", nullable: false),
                    FechaEvaluacion = table.Column<DateTime>(type: "datetime2", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_Evaluacion", x => x.IdEvaluacion);
                });

            migrationBuilder.CreateTable(
                name: "grupos",
                columns: table => new
                {
                    IdGrupo = table.Column<int>(type: "int", nullable: false)
                        .Annotation("SqlServer:Identity", "1, 1"),
                    NombreGrupo = table.Column<string>(type: "nvarchar(max)", nullable: false),
                    AnioLectivo = table.Column<int>(type: "int", nullable: false),
                    ciclo = table.Column<int>(type: "int", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_grupos", x => x.IdGrupo);
                });

            migrationBuilder.CreateTable(
                name: "Estudiantes",
                columns: table => new
                {
                    IdEstudiante = table.Column<int>(type: "int", nullable: false)
                        .Annotation("SqlServer:Identity", "1, 1"),
                    Carnet = table.Column<string>(type: "nvarchar(450)", nullable: false),
                    NombreCompleto = table.Column<string>(type: "nvarchar(max)", nullable: false),
                    IdGrupo = table.Column<int>(type: "int", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_Estudiantes", x => x.IdEstudiante);
                    table.ForeignKey(
                        name: "FK_Estudiantes_grupos_IdGrupo",
                        column: x => x.IdGrupo,
                        principalTable: "grupos",
                        principalColumn: "IdGrupo",
                        onDelete: ReferentialAction.Restrict);
                });

            migrationBuilder.CreateTable(
                name: "Evaluaciones",
                columns: table => new
                {
                    IdHistorial = table.Column<int>(type: "int", nullable: false)
                        .Annotation("SqlServer:Identity", "1, 1"),
                    PorcentajePlagio = table.Column<float>(type: "real", nullable: false),
                    PorcentajeIA = table.Column<float>(type: "real", nullable: false),
                    RutaArchivo = table.Column<string>(type: "nvarchar(max)", nullable: false),
                    MetadatosJson = table.Column<string>(type: "nvarchar(max)", nullable: false),
                    IdEstudiante = table.Column<int>(type: "int", nullable: false),
                    IdEvaluacion = table.Column<int>(type: "int", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_Evaluaciones", x => x.IdHistorial);
                    table.ForeignKey(
                        name: "FK_Evaluaciones_Estudiantes_IdEstudiante",
                        column: x => x.IdEstudiante,
                        principalTable: "Estudiantes",
                        principalColumn: "IdEstudiante",
                        onDelete: ReferentialAction.Cascade);
                    table.ForeignKey(
                        name: "FK_Evaluaciones_Evaluacion_IdEvaluacion",
                        column: x => x.IdEvaluacion,
                        principalTable: "Evaluacion",
                        principalColumn: "IdEvaluacion",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateIndex(
                name: "IX_Estudiantes_Carnet_IdGrupo",
                table: "Estudiantes",
                columns: new[] { "Carnet", "IdGrupo" },
                unique: true);

            migrationBuilder.CreateIndex(
                name: "IX_Estudiantes_IdGrupo",
                table: "Estudiantes",
                column: "IdGrupo");

            migrationBuilder.CreateIndex(
                name: "IX_Evaluaciones_IdEstudiante",
                table: "Evaluaciones",
                column: "IdEstudiante");

            migrationBuilder.CreateIndex(
                name: "IX_Evaluaciones_IdEvaluacion",
                table: "Evaluaciones",
                column: "IdEvaluacion");
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
