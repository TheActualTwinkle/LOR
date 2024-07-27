using System;
using Microsoft.EntityFrameworkCore.Migrations;
using Npgsql.EntityFrameworkCore.PostgreSQL.Metadata;

#nullable disable

namespace DatabaseApp.Persistence.Migrations
{
    /// <inheritdoc />
    public partial class Test3 : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.CreateTable(
                name: "Groups",
                columns: table => new
                {
                    id = table.Column<int>(type: "integer", nullable: false)
                        .Annotation("Npgsql:ValueGenerationStrategy", NpgsqlValueGenerationStrategy.IdentityAlwaysColumn),
                    group_name = table.Column<string>(type: "character varying", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("Groups_pkey", x => x.id);
                });

            migrationBuilder.CreateTable(
                name: "Classes",
                columns: table => new
                {
                    id = table.Column<int>(type: "integer", nullable: false)
                        .Annotation("Npgsql:ValueGenerationStrategy", NpgsqlValueGenerationStrategy.IdentityAlwaysColumn),
                    group_id = table.Column<int>(type: "integer", nullable: false),
                    class_name = table.Column<string>(type: "character varying", nullable: false),
                    date = table.Column<DateOnly>(type: "date", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("Classes_pkey", x => x.id);
                    table.ForeignKey(
                        name: "FK_Classes_Groups_group_id",
                        column: x => x.group_id,
                        principalTable: "Groups",
                        principalColumn: "id",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateTable(
                name: "Users",
                columns: table => new
                {
                    id = table.Column<int>(type: "integer", nullable: false),
                    full_name = table.Column<string>(type: "character varying", nullable: false, defaultValueSql: "0"),
                    telegram_id = table.Column<long>(type: "bigint", nullable: false, defaultValue: 0L),
                    GroupId = table.Column<int>(type: "integer", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("Users_pkey", x => x.id);
                    table.UniqueConstraint("AK_Users_telegram_id", x => x.telegram_id);
                    table.ForeignKey(
                        name: "FK_Users_Groups_GroupId",
                        column: x => x.GroupId,
                        principalTable: "Groups",
                        principalColumn: "id",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateTable(
                name: "Queue",
                columns: table => new
                {
                    id = table.Column<int>(type: "integer", nullable: false)
                        .Annotation("Npgsql:ValueGenerationStrategy", NpgsqlValueGenerationStrategy.IdentityAlwaysColumn),
                    queue_num = table.Column<long>(type: "bigint", nullable: false),
                    group_id = table.Column<int>(type: "integer", nullable: false),
                    class_id = table.Column<int>(type: "integer", nullable: false),
                    telegramm_id = table.Column<long>(type: "bigint", nullable: false, defaultValue: 0L)
                },
                constraints: table =>
                {
                    table.PrimaryKey("Queue_pkey", x => x.id);
                    table.ForeignKey(
                        name: "Queue_classes_id_fkey",
                        column: x => x.class_id,
                        principalTable: "Classes",
                        principalColumn: "id");
                    table.ForeignKey(
                        name: "Queue_group_id_fkey",
                        column: x => x.group_id,
                        principalTable: "Groups",
                        principalColumn: "id");
                    table.ForeignKey(
                        name: "Queue_tg_id_fkey",
                        column: x => x.telegramm_id,
                        principalTable: "Users",
                        principalColumn: "telegram_id");
                });

            migrationBuilder.CreateIndex(
                name: "IX_Classes_group_id",
                table: "Classes",
                column: "group_id");

            migrationBuilder.CreateIndex(
                name: "IX_Queue_class_id",
                table: "Queue",
                column: "class_id");

            migrationBuilder.CreateIndex(
                name: "IX_Queue_group_id",
                table: "Queue",
                column: "group_id");

            migrationBuilder.CreateIndex(
                name: "IX_Queue_telegramm_id",
                table: "Queue",
                column: "telegramm_id");

            migrationBuilder.CreateIndex(
                name: "full_name_check",
                table: "Users",
                column: "full_name",
                unique: true);

            migrationBuilder.CreateIndex(
                name: "IX_Users_GroupId",
                table: "Users",
                column: "GroupId");

            migrationBuilder.CreateIndex(
                name: "tg_id_check",
                table: "Users",
                column: "telegram_id",
                unique: true);
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropTable(
                name: "Queue");

            migrationBuilder.DropTable(
                name: "Classes");

            migrationBuilder.DropTable(
                name: "Users");

            migrationBuilder.DropTable(
                name: "Groups");
        }
    }
}
