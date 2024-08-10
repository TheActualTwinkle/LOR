using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace DatabaseApp.Persistence.Migrations
{
    /// <inheritdoc />
    public partial class Test11 : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropForeignKey(
                name: "FK_CLASSES_GROUPS_group_id",
                table: "CLASSES");

            migrationBuilder.DropForeignKey(
                name: "FK_USERS_GROUPS_GroupId",
                table: "USERS");

            migrationBuilder.RenameColumn(
                name: "GroupId",
                table: "USERS",
                newName: "group_id");

            migrationBuilder.RenameIndex(
                name: "IX_USERS_GroupId",
                table: "USERS",
                newName: "IX_USERS_group_id");

            migrationBuilder.AddForeignKey(
                name: "Class_group_id_fkey",
                table: "CLASSES",
                column: "group_id",
                principalTable: "GROUPS",
                principalColumn: "id");

            migrationBuilder.AddForeignKey(
                name: "User_group_id_fkey",
                table: "USERS",
                column: "group_id",
                principalTable: "GROUPS",
                principalColumn: "id");
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropForeignKey(
                name: "Class_group_id_fkey",
                table: "CLASSES");

            migrationBuilder.DropForeignKey(
                name: "User_group_id_fkey",
                table: "USERS");

            migrationBuilder.RenameColumn(
                name: "group_id",
                table: "USERS",
                newName: "GroupId");

            migrationBuilder.RenameIndex(
                name: "IX_USERS_group_id",
                table: "USERS",
                newName: "IX_USERS_GroupId");

            migrationBuilder.AddForeignKey(
                name: "FK_CLASSES_GROUPS_group_id",
                table: "CLASSES",
                column: "group_id",
                principalTable: "GROUPS",
                principalColumn: "id",
                onDelete: ReferentialAction.Cascade);

            migrationBuilder.AddForeignKey(
                name: "FK_USERS_GROUPS_GroupId",
                table: "USERS",
                column: "GroupId",
                principalTable: "GROUPS",
                principalColumn: "id",
                onDelete: ReferentialAction.Cascade);
        }
    }
}
