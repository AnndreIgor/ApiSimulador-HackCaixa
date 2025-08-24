using Microsoft.EntityFrameworkCore.Metadata;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace ApiSimulador.Migrations
{
    /// <inheritdoc />
    public partial class InitialCreate : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AlterDatabase()
                .Annotation("MySql:CharSet", "utf8mb4");

            migrationBuilder.CreateTable(
                name: "SIMULACAO",
                columns: table => new
                {
                    CO_SIMULACAO = table.Column<int>(type: "int", nullable: false)
                        .Annotation("MySql:ValueGenerationStrategy", MySqlValueGenerationStrategy.IdentityColumn),
                    PC_TAXA_JUROS = table.Column<decimal>(type: "decimal(65,30)", nullable: false),
                    PZ_SIMULACAO = table.Column<short>(type: "smallint", nullable: false),
                    VR_SIMULACAO = table.Column<decimal>(type: "decimal(65,30)", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_SIMULACAO", x => x.CO_SIMULACAO);
                })
                .Annotation("MySql:CharSet", "utf8mb4");

            migrationBuilder.CreateTable(
                name: "PARCELA",
                columns: table => new
                {
                    CO_PARCELA = table.Column<int>(type: "int", nullable: false)
                        .Annotation("MySql:ValueGenerationStrategy", MySqlValueGenerationStrategy.IdentityColumn),
                    NU_PARCELA = table.Column<short>(type: "smallint", nullable: false),
                    VR_AMORTIZACAO = table.Column<decimal>(type: "decimal(65,30)", nullable: false),
                    VR_JUROS = table.Column<decimal>(type: "decimal(65,30)", nullable: false),
                    VR_PRESTACAO = table.Column<decimal>(type: "decimal(65,30)", nullable: false),
                    TP_AMORTIZACAO = table.Column<string>(type: "longtext", nullable: false)
                        .Annotation("MySql:CharSet", "utf8mb4"),
                    CO_SIMULACAO = table.Column<int>(type: "int", nullable: false),
                    SimulacaoCO_SIMULACAO = table.Column<int>(type: "int", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_PARCELA", x => x.CO_PARCELA);
                    table.ForeignKey(
                        name: "FK_PARCELA_SIMULACAO_SimulacaoCO_SIMULACAO",
                        column: x => x.SimulacaoCO_SIMULACAO,
                        principalTable: "SIMULACAO",
                        principalColumn: "CO_SIMULACAO",
                        onDelete: ReferentialAction.Cascade);
                })
                .Annotation("MySql:CharSet", "utf8mb4");

            migrationBuilder.CreateIndex(
                name: "IX_PARCELA_SimulacaoCO_SIMULACAO",
                table: "PARCELA",
                column: "SimulacaoCO_SIMULACAO");
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropTable(
                name: "PARCELA");

            migrationBuilder.DropTable(
                name: "SIMULACAO");
        }
    }
}
