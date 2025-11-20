using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace CajaSanmiguel.Migrations
{
    /// <inheritdoc />
    public partial class Correccion1 : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.RenameColumn(
                name: "IdCliente",
                table: "Usuarios",
                newName: "IdUsuario");
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.RenameColumn(
                name: "IdUsuario",
                table: "Usuarios",
                newName: "IdCliente");
        }
    }
}
