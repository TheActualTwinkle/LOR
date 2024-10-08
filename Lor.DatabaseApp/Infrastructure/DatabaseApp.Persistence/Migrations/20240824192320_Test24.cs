﻿using System;
using Microsoft.EntityFrameworkCore.Migrations;
using Npgsql.EntityFrameworkCore.PostgreSQL.Metadata;

#nullable disable

namespace DatabaseApp.Persistence.Migrations
{
    /// <inheritdoc />
    public partial class Test24 : Migration
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
                    name = table.Column<string>(type: "text", nullable: false)
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
                    name = table.Column<string>(type: "text", nullable: false),
                    date = table.Column<DateOnly>(type: "date", nullable: false),
                    group_id = table.Column<int>(type: "integer", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("Classes_pkey", x => x.id);
                    table.ForeignKey(
                        name: "Class_group_id_fkey",
                        column: x => x.group_id,
                        principalTable: "GROUPS",
                        principalColumn: "id");
                });

            migrationBuilder.CreateTable(
                name: "USERS",
                columns: table => new
                {
                    id = table.Column<int>(type: "integer", nullable: false)
                        .Annotation("Npgsql:ValueGenerationStrategy", NpgsqlValueGenerationStrategy.IdentityAlwaysColumn),
                    full_name = table.Column<string>(type: "text", nullable: false),
                    telegram_id = table.Column<long>(type: "bigint", nullable: false),
                    group_id = table.Column<int>(type: "integer", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("Users_pkey", x => x.id);
                    table.ForeignKey(
                        name: "User_group_id_fkey",
                        column: x => x.group_id,
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
                    queue_num = table.Column<long>(type: "bigint", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("Queue_pkey", x => x.id);
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

            migrationBuilder.CreateTable(
                name: "SUBSCRIBERS",
                columns: table => new
                {
                    id = table.Column<int>(type: "integer", nullable: false)
                        .Annotation("Npgsql:ValueGenerationStrategy", NpgsqlValueGenerationStrategy.IdentityAlwaysColumn),
                    user_id = table.Column<int>(type: "integer", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("Subscriber_pkey", x => x.id);
                    table.ForeignKey(
                        name: "User_subscriber_id_fkey",
                        column: x => x.user_id,
                        principalTable: "USERS",
                        principalColumn: "id",
                        onDelete: ReferentialAction.Cascade);
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
                name: "IX_QUEUES_user_id",
                table: "QUEUES",
                column: "user_id");

            migrationBuilder.CreateIndex(
                name: "user_id_check",
                table: "SUBSCRIBERS",
                column: "user_id",
                unique: true);

            migrationBuilder.CreateIndex(
                name: "full_name_check",
                table: "USERS",
                column: "full_name",
                unique: true);

            migrationBuilder.CreateIndex(
                name: "IX_USERS_group_id",
                table: "USERS",
                column: "group_id");

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
                name: "SUBSCRIBERS");

            migrationBuilder.DropTable(
                name: "CLASSES");

            migrationBuilder.DropTable(
                name: "USERS");

            migrationBuilder.DropTable(
                name: "GROUPS");
        }
    }
}
