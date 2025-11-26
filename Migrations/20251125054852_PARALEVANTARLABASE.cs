using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace CajaSanmiguel.Migrations
{
    /// <inheritdoc />
    public partial class PARALEVANTARLABASE : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.CreateTable(
                name: "Clientes",
                columns: table => new
                {
                    IdCliente = table.Column<int>(type: "int", nullable: false)
                        .Annotation("SqlServer:Identity", "1, 1"),
                    Nombre = table.Column<string>(type: "nvarchar(max)", nullable: false),
                    Apellidos = table.Column<string>(type: "nvarchar(max)", nullable: false),
                    Telefono = table.Column<string>(type: "nvarchar(max)", nullable: false),
                    Direccion = table.Column<string>(type: "nvarchar(max)", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_Clientes", x => x.IdCliente);
                });

            migrationBuilder.CreateTable(
                name: "Usuarios",
                columns: table => new
                {
                    IdUsuario = table.Column<int>(type: "int", nullable: false)
                        .Annotation("SqlServer:Identity", "1, 1"),
                    Nombre = table.Column<string>(type: "nvarchar(max)", nullable: false),
                    Correo = table.Column<string>(type: "nvarchar(max)", nullable: false),
                    Password = table.Column<string>(type: "nvarchar(max)", nullable: false),
                    Rol = table.Column<string>(type: "nvarchar(max)", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_Usuarios", x => x.IdUsuario);
                });

            migrationBuilder.CreateTable(
                name: "Prestamos",
                columns: table => new
                {
                    IdPrestamo = table.Column<int>(type: "int", nullable: false)
                        .Annotation("SqlServer:Identity", "1, 1"),
                    IdCliente = table.Column<int>(type: "int", nullable: false),
                    Monto = table.Column<decimal>(type: "decimal(18,2)", nullable: false),
                    Interes = table.Column<decimal>(type: "decimal(18,2)", nullable: false),
                    NumeroCuotas = table.Column<int>(type: "int", nullable: false),
                    Lapzo = table.Column<string>(type: "nvarchar(max)", nullable: false),
                    MontoMulta = table.Column<decimal>(type: "decimal(18,2)", nullable: false),
                    TotalAPagar = table.Column<decimal>(type: "decimal(18,2)", nullable: false),
                    FechaInicio = table.Column<DateTime>(type: "datetime2", nullable: false),
                    Estado = table.Column<string>(type: "nvarchar(max)", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_Prestamos", x => x.IdPrestamo);
                    table.ForeignKey(
                        name: "FK_Prestamos_Clientes_IdCliente",
                        column: x => x.IdCliente,
                        principalTable: "Clientes",
                        principalColumn: "IdCliente",
                        onDelete: ReferentialAction.Restrict);
                });

            migrationBuilder.CreateTable(
                name: "Pagos",
                columns: table => new
                {
                    IdPago = table.Column<int>(type: "int", nullable: false)
                        .Annotation("SqlServer:Identity", "1, 1"),
                    IdPrestamo = table.Column<int>(type: "int", nullable: false),
                    NumeroCuota = table.Column<int>(type: "int", nullable: false),
                    FechaProgramada = table.Column<DateTime>(type: "datetime2", nullable: false),
                    MontoProgramado = table.Column<decimal>(type: "decimal(18,2)", nullable: false),
                    FechaPagoReal = table.Column<DateTime>(type: "datetime2", nullable: true),
                    MontoPagado = table.Column<decimal>(type: "decimal(18,2)", nullable: true),
                    Estado = table.Column<string>(type: "nvarchar(max)", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_Pagos", x => x.IdPago);
                    table.ForeignKey(
                        name: "FK_Pagos_Prestamos_IdPrestamo",
                        column: x => x.IdPrestamo,
                        principalTable: "Prestamos",
                        principalColumn: "IdPrestamo",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateIndex(
                name: "IX_Pagos_IdPrestamo",
                table: "Pagos",
                column: "IdPrestamo");

            migrationBuilder.CreateIndex(
                name: "IX_Prestamos_IdCliente",
                table: "Prestamos",
                column: "IdCliente");
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropTable(
                name: "Pagos");

            migrationBuilder.DropTable(
                name: "Usuarios");

            migrationBuilder.DropTable(
                name: "Prestamos");

            migrationBuilder.DropTable(
                name: "Clientes");
        }
    }
}
