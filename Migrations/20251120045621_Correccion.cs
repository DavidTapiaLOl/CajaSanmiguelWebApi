using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace CajaSanmiguel.Migrations
{
    /// <inheritdoc />
    public partial class Correccion : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropColumn(
                name: "IdPrestamo",
                table: "Pagos");
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AddColumn<int>(
                name: "IdPrestamo",
                table: "Pagos",
                type: "int",
                nullable: false,
                defaultValue: 0);
        }
    }
}
