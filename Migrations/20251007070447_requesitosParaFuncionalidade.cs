using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace blogger_backend.Migrations
{
    /// <inheritdoc />
    public partial class requesitosParaFuncionalidade : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropIndex(
                name: "IX_PesquisasCustomizadas_UsuarioId",
                table: "PesquisasCustomizadas");

            migrationBuilder.CreateIndex(
                name: "IX_Usuarios_Nome",
                table: "Usuarios",
                column: "Nome",
                unique: true);

            migrationBuilder.CreateIndex(
                name: "IX_PesquisasCustomizadas_UsuarioId_CategoriaId_AutorId_FonteId",
                table: "PesquisasCustomizadas",
                columns: new[] { "UsuarioId", "CategoriaId", "AutorId", "FonteId" },
                unique: true);

            migrationBuilder.CreateIndex(
                name: "IX_Fontes_Nome",
                table: "Fontes",
                column: "Nome",
                unique: true);

            migrationBuilder.CreateIndex(
                name: "IX_Fontes_URL",
                table: "Fontes",
                column: "URL",
                unique: true);

            migrationBuilder.CreateIndex(
                name: "IX_Categorias_Nome",
                table: "Categorias",
                column: "Nome",
                unique: true);

            migrationBuilder.CreateIndex(
                name: "IX_Categorias_Slug",
                table: "Categorias",
                column: "Slug",
                unique: true);

            migrationBuilder.CreateIndex(
                name: "IX_Autores_Email",
                table: "Autores",
                column: "Email",
                unique: true);

            migrationBuilder.CreateIndex(
                name: "IX_Autores_Nome",
                table: "Autores",
                column: "Nome",
                unique: true);
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropIndex(
                name: "IX_Usuarios_Nome",
                table: "Usuarios");

            migrationBuilder.DropIndex(
                name: "IX_PesquisasCustomizadas_UsuarioId_CategoriaId_AutorId_FonteId",
                table: "PesquisasCustomizadas");

            migrationBuilder.DropIndex(
                name: "IX_Fontes_Nome",
                table: "Fontes");

            migrationBuilder.DropIndex(
                name: "IX_Fontes_URL",
                table: "Fontes");

            migrationBuilder.DropIndex(
                name: "IX_Categorias_Nome",
                table: "Categorias");

            migrationBuilder.DropIndex(
                name: "IX_Categorias_Slug",
                table: "Categorias");

            migrationBuilder.DropIndex(
                name: "IX_Autores_Email",
                table: "Autores");

            migrationBuilder.DropIndex(
                name: "IX_Autores_Nome",
                table: "Autores");

            migrationBuilder.CreateIndex(
                name: "IX_PesquisasCustomizadas_UsuarioId",
                table: "PesquisasCustomizadas",
                column: "UsuarioId");
        }
    }
}
