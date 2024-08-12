using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace DatabaseApp.Persistence.Migrations
{
    /// <inheritdoc />
    public partial class Test12 : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropForeignKey(
                name: "User_group_id_fkey",
                table: "USERS");

            migrationBuilder.RenameColumn(
                name: "group_name",
                table: "GROUPS",
                newName: "name");

            migrationBuilder.RenameColumn(
                name: "class_name",
                table: "CLASSES",
                newName: "name");

            migrationBuilder.AddForeignKey(
                name: "User_group_id_fkey",
                table: "USERS",
                column: "group_id",
                principalTable: "GROUPS",
                principalColumn: "id",
                onDelete: ReferentialAction.Cascade);
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropForeignKey(
                name: "User_group_id_fkey",
                table: "USERS");

            migrationBuilder.RenameColumn(
                name: "name",
                table: "GROUPS",
                newName: "group_name");

            migrationBuilder.RenameColumn(
                name: "name",
                table: "CLASSES",
                newName: "class_name");

            migrationBuilder.AddForeignKey(
                name: "User_group_id_fkey",
                table: "USERS",
                column: "group_id",
                principalTable: "GROUPS",
                principalColumn: "id");
        }
    }
}
