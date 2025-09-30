using blogger_backend.Models;
using Microsoft.AspNetCore.Identity;

namespace blogger_backend.Data
{
    public static class DbInitializer
    {
        public static void Seed(AppDbContext context, IPasswordHasher<UsuarioModel> hasher)
        {
            // === Categorias ===
            if (!context.Categorias.Any())
            {
                context.Categorias.AddRange(
                    new CategoriaModel { Nome = "Tecnologia" },
                    new CategoriaModel { Nome = "Educação" },
                    new CategoriaModel { Nome = "Ciência" },
                    new CategoriaModel { Nome = "Esportes" },
                    new CategoriaModel { Nome = "Saúde" }
                );
                context.SaveChanges();
            }

            // === Fontes ===
            if (!context.Fontes.Any())
            {
                context.Fontes.AddRange(
                    new FonteModel { Nome = "Agência Angola Press" },
                    new FonteModel { Nome = "BBC" },
                    new FonteModel { Nome = "CNN" },
                    new FonteModel { Nome = "The Guardian" },
                    new FonteModel { Nome = "DW África" }
                );
                context.SaveChanges();
            }

            // === Autores ===
            if (!context.Autores.Any())
            {
                context.Autores.AddRange(
                    new AutorModel { Nome = "João Silva" },
                    new AutorModel { Nome = "Maria Oliveira" },
                    new AutorModel { Nome = "Carlos Mendes" },
                    new AutorModel { Nome = "Ana Costa" },
                    new AutorModel { Nome = "Pedro Gomes" }
                );
                context.SaveChanges();
            }

            // === Usuários (com senha hasheada) ===
            if (!context.Usuarios.Any())
            {
                var admin = new UsuarioModel
                {
                    Nome = "Admin",
                    Email = "admin@email.com",
                    Role = "Admin",
                    Ativo = true,
                    DataCadastro = DateTime.UtcNow
                };
                admin.SenhaHash = hasher.HashPassword(admin, "123456");

                var joao = new UsuarioModel
                {
                    Nome = "João",
                    Email = "joao@email.com",
                    Role = "User",
                    Ativo = true,
                    DataCadastro = DateTime.UtcNow
                };
                joao.SenhaHash = hasher.HashPassword(joao, "123456");

                var maria = new UsuarioModel
                {
                    Nome = "Maria",
                    Email = "maria@email.com",
                    Role = "User",
                    Ativo = true,
                    DataCadastro = DateTime.UtcNow
                };
                maria.SenhaHash = hasher.HashPassword(maria, "123456");

                context.Usuarios.AddRange(admin, joao, maria);
                context.SaveChanges();
            }

            // === Artigos relacionados ===
            if (!context.Artigos.Any())
            {
                var categoria = context.Categorias.First();
                var autor = context.Autores.First();
                var fonte = context.Fontes.First();

                context.Artigos.AddRange(
                    new ArtigoModel
                    {
                        Titulo = "Primeiro Artigo",
                        Slug = "primeiro-artigo",
                        Conteudo = "Conteúdo exemplo...",
                        Resumo = "Resumo do artigo",
                        CategoriaId = categoria.Id,
                        AutorId = autor.Id,
                        FonteId = fonte.Id,
                        IsPublicado = true,
                        DataCriacao = DateTime.UtcNow
                    },
                    new ArtigoModel
                    {
                        Titulo = "Segundo Artigo",
                        Slug = "segundo-artigo",
                        Conteudo = "Outro conteúdo...",
                        Resumo = "Resumo do segundo artigo",
                        CategoriaId = categoria.Id,
                        AutorId = autor.Id,
                        FonteId = fonte.Id,
                        IsPublicado = true,
                        DataCriacao = DateTime.UtcNow
                    }
                );
                context.SaveChanges();
            }
        }
    }
}
