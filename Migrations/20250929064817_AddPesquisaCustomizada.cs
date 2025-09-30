using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace blogger_backend.Migrations
{
    /// <inheritdoc />
    public partial class AddPesquisaCustomizada : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.CreateTable(
                name: "PesquisasCustomizadas",
                columns: table => new
                {
                    Id = table.Column<int>(type: "INTEGER", nullable: false)
                        .Annotation("Sqlite:Autoincrement", true),
                    UsuarioId = table.Column<int>(type: "INTEGER", nullable: false),
                    CategoriaId = table.Column<int>(type: "INTEGER", nullable: true),
                    AutorId = table.Column<int>(type: "INTEGER", nullable: true),
                    FonteId = table.Column<int>(type: "INTEGER", nullable: true),
                    DataCriacao = table.Column<DateTime>(type: "TEXT", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_PesquisasCustomizadas", x => x.Id);
                    table.ForeignKey(
                        name: "FK_PesquisasCustomizadas_Autores_AutorId",
                        column: x => x.AutorId,
                        principalTable: "Autores",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Restrict);
                    table.ForeignKey(
                        name: "FK_PesquisasCustomizadas_Categorias_CategoriaId",
                        column: x => x.CategoriaId,
                        principalTable: "Categorias",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Restrict);
                    table.ForeignKey(
                        name: "FK_PesquisasCustomizadas_Fontes_FonteId",
                        column: x => x.FonteId,
                        principalTable: "Fontes",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Restrict);
                    table.ForeignKey(
                        name: "FK_PesquisasCustomizadas_Usuarios_UsuarioId",
                        column: x => x.UsuarioId,
                        principalTable: "Usuarios",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateIndex(
                name: "IX_PesquisasCustomizadas_AutorId",
                table: "PesquisasCustomizadas",
                column: "AutorId");

            migrationBuilder.CreateIndex(
                name: "IX_PesquisasCustomizadas_CategoriaId",
                table: "PesquisasCustomizadas",
                column: "CategoriaId");

            migrationBuilder.CreateIndex(
                name: "IX_PesquisasCustomizadas_FonteId",
                table: "PesquisasCustomizadas",
                column: "FonteId");

            migrationBuilder.CreateIndex(
                name: "IX_PesquisasCustomizadas_UsuarioId",
                table: "PesquisasCustomizadas",
                column: "UsuarioId");
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropTable(
                name: "PesquisasCustomizadas");
        }
    }
}
