using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace DatabaseApp.Persistence.Migrations
{
    /// <inheritdoc />
    public partial class EmailAttributes : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropIndex(
                name: "full_name_check",
                table: "USERS");

            migrationBuilder.AddColumn<string>(
                name: "email",
                table: "USERS",
                type: "text",
                nullable: false,
                defaultValue: "");

            migrationBuilder.AddColumn<bool>(
                name: "is_email_confirmed",
                table: "USERS",
                type: "boolean",
                nullable: false,
                defaultValueSql: "false");

            migrationBuilder.CreateIndex(
                name: "email_check",
                table: "USERS",
                column: "email",
                unique: true);
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropIndex(
                name: "email_check",
                table: "USERS");

            migrationBuilder.DropColumn(
                name: "email",
                table: "USERS");

            migrationBuilder.DropColumn(
                name: "is_email_confirmed",
                table: "USERS");

            migrationBuilder.CreateIndex(
                name: "full_name_check",
                table: "USERS",
                column: "full_name",
                unique: true);
        }
    }
}
