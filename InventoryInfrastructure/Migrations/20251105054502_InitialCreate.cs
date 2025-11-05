using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace InventoryDatabase.Migrations
{
    /// <inheritdoc />
    public partial class InitialCreate : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.CreateTable(
                name: "DocumentosTransporte",
                columns: table => new
                {
                    TrasnporDocumentId = table.Column<int>(type: "int", nullable: false)
                        .Annotation("SqlServer:Identity", "1, 1"),
                    Codigo = table.Column<string>(type: "nvarchar(max)", nullable: false),
                    Origen = table.Column<string>(type: "nvarchar(max)", nullable: false),
                    Estado = table.Column<string>(type: "nvarchar(max)", nullable: false),
                    FechaCreacion = table.Column<DateTime>(type: "datetime2", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_DocumentosTransporte", x => x.TrasnporDocumentId);
                });

            migrationBuilder.CreateTable(
                name: "Cajas",
                columns: table => new
                {
                    CajaId = table.Column<int>(type: "int", nullable: false)
                        .Annotation("SqlServer:Identity", "1, 1"),
                    CodigoCaja = table.Column<string>(type: "nvarchar(max)", nullable: false),
                    DocumentoTransporteId = table.Column<int>(type: "int", nullable: false),
                    CantidadReferencias = table.Column<int>(type: "int", nullable: false),
                    CantidadBackorder = table.Column<int>(type: "int", nullable: false),
                    Estado = table.Column<string>(type: "nvarchar(max)", nullable: false),
                    FechaInsercion = table.Column<DateTime>(type: "datetime2", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_Cajas", x => x.CajaId);
                    table.ForeignKey(
                        name: "FK_Cajas_DocumentosTransporte_DocumentoTransporteId",
                        column: x => x.DocumentoTransporteId,
                        principalTable: "DocumentosTransporte",
                        principalColumn: "TrasnporDocumentId",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateTable(
                name: "Referencias",
                columns: table => new
                {
                    ReferenciaId = table.Column<int>(type: "int", nullable: false)
                        .Annotation("SqlServer:Identity", "1, 1"),
                    CajaId = table.Column<int>(type: "int", nullable: false),
                    CodigoReferencia = table.Column<string>(type: "nvarchar(max)", nullable: false),
                    NombreReferencia = table.Column<string>(type: "nvarchar(max)", nullable: false),
                    TieneBackorder = table.Column<bool>(type: "bit", nullable: false),
                    Estado = table.Column<string>(type: "nvarchar(max)", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_Referencias", x => x.ReferenciaId);
                    table.ForeignKey(
                        name: "FK_Referencias_Cajas_CajaId",
                        column: x => x.CajaId,
                        principalTable: "Cajas",
                        principalColumn: "CajaId",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateIndex(
                name: "IX_Cajas_DocumentoTransporteId",
                table: "Cajas",
                column: "DocumentoTransporteId");

            migrationBuilder.CreateIndex(
                name: "IX_Referencias_CajaId",
                table: "Referencias",
                column: "CajaId");
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropTable(
                name: "Referencias");

            migrationBuilder.DropTable(
                name: "Cajas");

            migrationBuilder.DropTable(
                name: "DocumentosTransporte");
        }
    }
}
