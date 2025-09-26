using Microsoft.EntityFrameworkCore;
using blogger_backend.Models;
using blogger_backend.Data;

namespace blogger_backend.Routes;

public static class ArtigoRoute
{
    public static void ArtigoRoutes(this WebApplication app)
    {
        var route = app.MapGroup("/artigo");

        // POST
        route.MapPost("", async (ArtigoRequest req, AppDbContext context) =>
        {
            var artigo = new ArtigoModel
            {
                Titulo = req.Titulo,
                Slug = req.Slug,
                Conteudo = req.Conteudo,
                Resumo = req.Resumo,
                DataCriacao = DateTime.UtcNow,
                DataAtualizacao = DateTime.UtcNow,
                IsPublicado = req.IsPublicado,
                CategoriaId = req.CategoriaId,
                AutorId = req.AutorId,
                FonteId = req.FonteId
            };

            await context.Artigos.AddAsync(artigo);
            await context.SaveChangesAsync();
            return Results.Created($"/artigo/{artigo.Id}", artigo);
        });

        // GET
        route.MapGet("", async (AppDbContext context) =>
        {
            var artigos = await context.Artigos
                .Include(a => a.Autor)
                .Include(a => a.Categoria)
                .Include(a => a.Fonte)
                .ToListAsync();
            return Results.Ok(artigos);
        });

        // PUT
        route.MapPut("/{id:int}", async (int id, ArtigoRequest req, AppDbContext context) =>
        {
            var artigo = await context.Artigos.FirstOrDefaultAsync(a => a.Id == id);
            if (artigo == null) return Results.NotFound();

            artigo.Titulo = req.Titulo;
            artigo.Slug = req.Slug;
            artigo.Conteudo = req.Conteudo;
            artigo.Resumo = req.Resumo;
            artigo.DataAtualizacao = DateTime.UtcNow;
            artigo.IsPublicado = req.IsPublicado;
            artigo.CategoriaId = req.CategoriaId;
            artigo.AutorId = req.AutorId;
            artigo.FonteId = req.FonteId;

            await context.SaveChangesAsync();
            return Results.Ok(artigo);
        });

        // DELETE (soft delete)
        route.MapDelete("/{id:int}", async (int id, AppDbContext context) =>
        {
            var artigo = await context.Artigos.FirstOrDefaultAsync(a => a.Id == id);
            if (artigo == null) return Results.NotFound();

            artigo.IsPublicado = false; // Soft delete
            await context.SaveChangesAsync();
            return Results.Ok();
        });
    }
}
