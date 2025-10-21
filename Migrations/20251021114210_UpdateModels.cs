using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace blogger_backend.Migrations
{
    /// <inheritdoc />
    public partial class UpdateModels : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropForeignKey(
                name: "FK_Authores_Users_UserId",
                table: "Authores");

            migrationBuilder.DropIndex(
                name: "IX_Authores_UserId",
                table: "Authores");

            migrationBuilder.CreateIndex(
                name: "IX_Authores_UserId",
                table: "Authores",
                column: "UserId",
                unique: true);

            migrationBuilder.AddForeignKey(
                name: "FK_Authores_Users_UserId",
                table: "Authores",
                column: "UserId",
                principalTable: "Users",
                principalColumn: "Id",
                onDelete: ReferentialAction.Cascade);
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropForeignKey(
                name: "FK_Authores_Users_UserId",
                table: "Authores");

            migrationBuilder.DropIndex(
                name: "IX_Authores_UserId",
                table: "Authores");

            migrationBuilder.CreateIndex(
                name: "IX_Authores_UserId",
                table: "Authores",
                column: "UserId");

            migrationBuilder.AddForeignKey(
                name: "FK_Authores_Users_UserId",
                table: "Authores",
                column: "UserId",
                principalTable: "Users",
                principalColumn: "Id");
        }
    }
}
