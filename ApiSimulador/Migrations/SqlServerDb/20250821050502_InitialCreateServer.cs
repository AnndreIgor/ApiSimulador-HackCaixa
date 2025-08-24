using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace ApiSimulador.Migrations.SqlServerDb
{
    /// <inheritdoc />
    public partial class InitialCreateServer : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.CreateTable(
                name: "PRODUTO",
                columns: table => new
                {
                    CO_PRODUTO = table.Column<int>(type: "int", nullable: false)
                        .Annotation("SqlServer:Identity", "1, 1"),
                    NO_PRODUTO = table.Column<string>(type: "nvarchar(max)", nullable: false),
                    PC_TAXA_JUROS = table.Column<decimal>(type: "decimal(18,2)", nullable: false),
                    NU_MINIMO_MESES = table.Column<short>(type: "smallint", nullable: false),
                    NU_MAXIMO_MESES = table.Column<short>(type: "smallint", nullable: false),
                    VR_MINIMO = table.Column<decimal>(type: "decimal(18,2)", nullable: false),
                    VR_MAXIMO = table.Column<decimal>(type: "decimal(18,2)", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_PRODUTO", x => x.CO_PRODUTO);
                });
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropTable(
                name: "PRODUTO");
        }
    }
}
