using Microsoft.EntityFrameworkCore.Migrations;
using Npgsql.EntityFrameworkCore.PostgreSQL.Metadata;

#nullable disable

namespace DatabaseApp.Persistence.Migrations
{
    /// <inheritdoc />
    public partial class Test16 : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.RenameIndex(
                name: "tg_id_check",
                table: "USERS",
                newName: "tg_id_check1");

            migrationBuilder.AddUniqueConstraint(
                name: "AK_USERS_telegram_id",
                table: "USERS",
                column: "telegram_id");

            migrationBuilder.CreateTable(
                name: "SUBSCRIBERS",
                columns: table => new
                {
                    id = table.Column<int>(type: "integer", nullable: false)
                        .Annotation("Npgsql:ValueGenerationStrategy", NpgsqlValueGenerationStrategy.IdentityAlwaysColumn),
                    telegram_id = table.Column<long>(type: "bigint", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("Subscriber_pkey", x => x.id);
                    table.ForeignKey(
                        name: "Subscriber_user_id_fkey",
                        column: x => x.telegram_id,
                        principalTable: "USERS",
                        principalColumn: "telegram_id",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateIndex(
                name: "tg_id_check",
                table: "SUBSCRIBERS",
                column: "telegram_id",
                unique: true);
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropTable(
                name: "SUBSCRIBERS");

            migrationBuilder.DropUniqueConstraint(
                name: "AK_USERS_telegram_id",
                table: "USERS");

            migrationBuilder.RenameIndex(
                name: "tg_id_check1",
                table: "USERS",
                newName: "tg_id_check");
        }
    }
}
