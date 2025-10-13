using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace blogger_backend.Migrations
{
    /// <inheritdoc />
    public partial class UpdateAuthorValidation4 : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropIndex(
                name: "IX_CustomizedResearches_UserId_CategoryId_AuthorId_SourceId",
                table: "CustomizedResearches");

            migrationBuilder.CreateIndex(
                name: "IX_CustomizedResearches_UserId_AuthorId",
                table: "CustomizedResearches",
                columns: new[] { "UserId", "AuthorId" },
                unique: true,
                filter: "\"AuthorId\" IS NOT NULL");

            migrationBuilder.CreateIndex(
                name: "IX_CustomizedResearches_UserId_CategoryId",
                table: "CustomizedResearches",
                columns: new[] { "UserId", "CategoryId" },
                unique: true,
                filter: "\"CategoryId\" IS NOT NULL");

            migrationBuilder.CreateIndex(
                name: "IX_CustomizedResearches_UserId_SourceId",
                table: "CustomizedResearches",
                columns: new[] { "UserId", "SourceId" },
                unique: true,
                filter: "\"SourceId\" IS NOT NULL");
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropIndex(
                name: "IX_CustomizedResearches_UserId_AuthorId",
                table: "CustomizedResearches");

            migrationBuilder.DropIndex(
                name: "IX_CustomizedResearches_UserId_CategoryId",
                table: "CustomizedResearches");

            migrationBuilder.DropIndex(
                name: "IX_CustomizedResearches_UserId_SourceId",
                table: "CustomizedResearches");

            migrationBuilder.CreateIndex(
                name: "IX_CustomizedResearches_UserId_CategoryId_AuthorId_SourceId",
                table: "CustomizedResearches",
                columns: new[] { "UserId", "CategoryId", "AuthorId", "SourceId" },
                unique: true);
        }
    }
}
