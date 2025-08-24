using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace ApiSimulador.Migrations
{
    /// <inheritdoc />
    public partial class NoMigration3 : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AddColumn<int>(
                name: "CO_PRODUTO",
                table: "SIMULACAO",
                type: "int",
                nullable: false,
                defaultValue: 0);
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropColumn(
                name: "CO_PRODUTO",
                table: "SIMULACAO");
        }
    }
}
