using Microsoft.EntityFrameworkCore.Migrations;
using Npgsql.EntityFrameworkCore.PostgreSQL.Metadata;

#nullable disable

namespace TrnGeneratorApi.Migrations;

public partial class InitialCreate : Migration
{
    protected override void Up(MigrationBuilder migrationBuilder)
    {
        migrationBuilder.CreateTable(
            name: "trn_range",
            columns: table => new
            {
                from_trn = table.Column<int>(type: "integer", nullable: false)
                    .Annotation("Npgsql:ValueGenerationStrategy", NpgsqlValueGenerationStrategy.IdentityByDefaultColumn),
                to_trn = table.Column<int>(type: "integer", nullable: false),
                next_trn = table.Column<int>(type: "integer", nullable: false),
                is_exhausted = table.Column<bool>(type: "boolean", nullable: false)
            },
            constraints: table =>
            {
                table.PrimaryKey("pk_trn_range", x => x.from_trn);
            });

        migrationBuilder.CreateIndex(
            name: "ix_trn_range_unexhausted_trn_ranges",
            table: "trn_range",
            column: "from_trn",
            filter: "is_exhausted IS FALSE");
    }

    protected override void Down(MigrationBuilder migrationBuilder)
    {
        migrationBuilder.DropTable(
            name: "trn_range");
    }
}
