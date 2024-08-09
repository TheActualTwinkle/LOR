using System;
using Microsoft.EntityFrameworkCore.Migrations;
using Npgsql.EntityFrameworkCore.PostgreSQL.Metadata;

#nullable disable

namespace DatabaseApp.Persistence.Migrations
{
    /// <inheritdoc />
    public partial class Test10 : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.CreateTable(
                name: "GROUPS",
                columns: table => new
                {
                    id = table.Column<int>(type: "integer", nullable: false)
                        .Annotation("Npgsql:ValueGenerationStrategy", NpgsqlValueGenerationStrategy.IdentityAlwaysColumn),
                    group_name = table.Column<string>(type: "text", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("Groups_pkey", x => x.id);
                });

            migrationBuilder.CreateTable(
                name: "CLASSES",
                columns: table => new
                {
                    id = table.Column<int>(type: "integer", nullable: false)
                        .Annotation("Npgsql:ValueGenerationStrategy", NpgsqlValueGenerationStrategy.IdentityAlwaysColumn),
                    group_id = table.Column<int>(type: "integer", nullable: false),
                    class_name = table.Column<string>(type: "text", nullable: false),
                    date = table.Column<DateOnly>(type: "date", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("Classes_pkey", x => x.id);
                    table.ForeignKey(
                        name: "FK_CLASSES_GROUPS_group_id",
                        column: x => x.group_id,
                        principalTable: "GROUPS",
                        principalColumn: "id",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateTable(
                name: "USERS",
                columns: table => new
                {
                    id = table.Column<int>(type: "integer", nullable: false)
                        .Annotation("Npgsql:ValueGenerationStrategy", NpgsqlValueGenerationStrategy.IdentityAlwaysColumn),
                    full_name = table.Column<string>(type: "text", nullable: false),
                    telegram_id = table.Column<long>(type: "bigint", nullable: false),
                    GroupId = table.Column<int>(type: "integer", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("Users_pkey", x => x.id);
                    table.ForeignKey(
                        name: "FK_USERS_GROUPS_GroupId",
                        column: x => x.GroupId,
                        principalTable: "GROUPS",
                        principalColumn: "id",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateTable(
                name: "QUEUES",
                columns: table => new
                {
                    id = table.Column<int>(type: "integer", nullable: false)
                        .Annotation("Npgsql:ValueGenerationStrategy", NpgsqlValueGenerationStrategy.IdentityAlwaysColumn),
                    class_id = table.Column<int>(type: "integer", nullable: false),
                    user_id = table.Column<int>(type: "integer", nullable: false),
                    queue_num = table.Column<long>(type: "bigint", nullable: false),
                    GroupId = table.Column<int>(type: "integer", nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("Queue_pkey", x => x.id);
                    table.ForeignKey(
                        name: "FK_QUEUES_GROUPS_GroupId",
                        column: x => x.GroupId,
                        principalTable: "GROUPS",
                        principalColumn: "id");
                    table.ForeignKey(
                        name: "Queue_classes_id_fkey",
                        column: x => x.class_id,
                        principalTable: "CLASSES",
                        principalColumn: "id");
                    table.ForeignKey(
                        name: "Queue_user_id_fkey",
                        column: x => x.user_id,
                        principalTable: "USERS",
                        principalColumn: "id");
                });

            migrationBuilder.CreateIndex(
                name: "IX_CLASSES_group_id",
                table: "CLASSES",
                column: "group_id");

            migrationBuilder.CreateIndex(
                name: "IX_QUEUES_class_id",
                table: "QUEUES",
                column: "class_id");

            migrationBuilder.CreateIndex(
                name: "IX_QUEUES_GroupId",
                table: "QUEUES",
                column: "GroupId");

            migrationBuilder.CreateIndex(
                name: "IX_QUEUES_user_id",
                table: "QUEUES",
                column: "user_id");

            migrationBuilder.CreateIndex(
                name: "full_name_check",
                table: "USERS",
                column: "full_name",
                unique: true);

            migrationBuilder.CreateIndex(
                name: "IX_USERS_GroupId",
                table: "USERS",
                column: "GroupId");

            migrationBuilder.CreateIndex(
                name: "tg_id_check",
                table: "USERS",
                column: "telegram_id",
                unique: true);
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropTable(
                name: "QUEUES");

            migrationBuilder.DropTable(
                name: "CLASSES");

            migrationBuilder.DropTable(
                name: "USERS");

            migrationBuilder.DropTable(
                name: "GROUPS");
        }
    }
}
