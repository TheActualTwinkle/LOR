using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace DatabaseApp.Persistence.Migrations
{
    /// <inheritdoc />
    public partial class Test18 : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropForeignKey(
                name: "Subscriber_user_id_fkey",
                table: "SUBSCRIBERS");

            migrationBuilder.AddUniqueConstraint(
                name: "AK_SUBSCRIBERS_telegram_id",
                table: "SUBSCRIBERS",
                column: "telegram_id");

            migrationBuilder.AddForeignKey(
                name: "User_subscriber_id_fkey",
                table: "USERS",
                column: "telegram_id",
                principalTable: "SUBSCRIBERS",
                principalColumn: "telegram_id",
                onDelete: ReferentialAction.Cascade);
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropForeignKey(
                name: "User_subscriber_id_fkey",
                table: "USERS");

            migrationBuilder.DropUniqueConstraint(
                name: "AK_SUBSCRIBERS_telegram_id",
                table: "SUBSCRIBERS");

            migrationBuilder.AddForeignKey(
                name: "Subscriber_user_id_fkey",
                table: "SUBSCRIBERS",
                column: "telegram_id",
                principalTable: "USERS",
                principalColumn: "telegram_id",
                onDelete: ReferentialAction.Cascade);
        }
    }
}
