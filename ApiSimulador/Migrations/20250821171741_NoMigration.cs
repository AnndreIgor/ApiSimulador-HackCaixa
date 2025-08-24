using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace ApiSimulador.Migrations
{
    /// <inheritdoc />
    public partial class NoMigration : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AlterColumn<decimal>(
                name: "VR_SIMULACAO",
                table: "SIMULACAO",
                type: "decimal(18,2)",
                nullable: false,
                oldClrType: typeof(decimal),
                oldType: "decimal(65,30)");

            migrationBuilder.AlterColumn<decimal>(
                name: "PC_TAXA_JUROS",
                table: "SIMULACAO",
                type: "decimal(10,9)",
                nullable: false,
                oldClrType: typeof(decimal),
                oldType: "decimal(65,30)");
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AlterColumn<decimal>(
                name: "VR_SIMULACAO",
                table: "SIMULACAO",
                type: "decimal(65,30)",
                nullable: false,
                oldClrType: typeof(decimal),
                oldType: "decimal(18,2)");

            migrationBuilder.AlterColumn<decimal>(
                name: "PC_TAXA_JUROS",
                table: "SIMULACAO",
                type: "decimal(65,30)",
                nullable: false,
                oldClrType: typeof(decimal),
                oldType: "decimal(10,9)");
        }
    }
}
