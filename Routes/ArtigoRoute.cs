using Microsoft.EntityFrameworkCore;
using blogger_backend.Models;
using blogger_backend.Data;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Http;

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
                FonteId = req.FonteId,
                Imagem = req.Imagem
            };

            await context.Artigos.AddAsync(artigo);
            await context.SaveChangesAsync();
            return Results.Created($"/artigo/{artigo.Id}", artigo);
        });
        // GET
        route.MapGet("completo", async (AppDbContext context) =>
        {
            var artigos = await context.Artigos
                                    .Include(a => a.Categoria)
                                    .Include(a => a.Autor)
                                    .Include(a => a.Fonte)
                                    .Where(a => a.IsPublicado)
                                    .Select(a => new ArtigoResponse(

                                        a.Titulo,
                                        a.Slug,
                                        a.Conteudo,
                                        a.Resumo,
                                        a.Imagem,
                                        a.Autor.Nome,
                                        a.DataCriacao
                                    ))
                                    .ToListAsync();

            return Results.Ok(artigos);
        });
        // GET simplificado (lista de artigos com menos dados)
        route.MapGet("/paginaInical", async (AppDbContext context) =>
        {
            var artigos = await context.Artigos
                                    .Include(a => a.Categoria)
                                    .Include(a => a.Autor)
                                    .Include(a => a.Fonte)
                                    .Where(a => a.IsPublicado)
                                    .Select(a => new
                                    {
                                        a.Titulo,
                                        a.Imagem,
                                        a.DataCriacao,
                                        Autor = a.Autor.Nome,
                                        a.Resumo
                                    })
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
            artigo.Imagem = req.Imagem;

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
        // GET Pesquisa avançada
        route.MapGet("/pesquisar", async (
   string? titulo,
   [FromQuery] int[]? categorias,
   [FromQuery] int[]? fontes,
   DateTime? data,
   AppDbContext context) =>
{
    var query = context.Artigos
        .Include(a => a.Categoria)
        .Include(a => a.Autor)
        .Include(a => a.Fonte)
        .Where(a => a.IsPublicado)
        .AsQueryable();

    // Se nenhum filtro for passado, retorna todos publicados
    if (string.IsNullOrWhiteSpace(titulo) &&
        (categorias == null || !categorias.Any()) &&
        (fontes == null || !fontes.Any()) &&
        !data.HasValue)
    {
        return Results.Ok(await query
            .Select(a => new
            {
                a.Titulo,
                a.Imagem,
                a.DataCriacao,
                Autor = a.Autor.Nome,
                a.Resumo
            }).ToListAsync());
    }

    // Filtro OR – pelo menos um critério deve bater
    query = query.Where(a =>
        (!string.IsNullOrWhiteSpace(titulo) && a.Titulo.ToLower().Contains(titulo.ToLower())) ||
        (categorias != null && categorias.Contains(a.CategoriaId)) ||
        (fontes != null && a.FonteId != null && fontes.Contains(a.FonteId.Value)) ||
        (data.HasValue && a.DataCriacao.Date == data.Value.Date) // compara apenas a data
    );

    var artigos = await query
        .Select(a => new
        {
            a.Titulo,
            a.Imagem,
            a.DataCriacao,
            Autor = a.Autor.Nome,
            a.Resumo
        })
        .ToListAsync();

    // Logs de debug
    Console.WriteLine($"Titulo: {titulo}");
    Console.WriteLine($"Categorias: {(categorias == null ? "Nenhuma" : string.Join(",", categorias))}");
    Console.WriteLine($"Fontes: {(fontes == null ? "Nenhuma" : string.Join(",", fontes))}");
    Console.WriteLine($"Data: {data}");
    Console.WriteLine($"Artigos retornados: {artigos.Count}");

    return Results.Ok(artigos);
});
        // POST upload imagem
        route.MapPost("/upload-imagem", async (HttpRequest request) =>
        {
            var form = await request.ReadFormAsync();
            var file = form.Files.FirstOrDefault();

            if (file == null || file.Length == 0)
                return Results.BadRequest("Nenhuma imagem enviada.");

            // Caminho da pasta de upload
            var uploadsPath = Path.Combine(Directory.GetCurrentDirectory(), "wwwroot", "uploads", "artigos");
            if (!Directory.Exists(uploadsPath))
                Directory.CreateDirectory(uploadsPath);

            // Nome único para a imagem
            var fileName = Guid.NewGuid().ToString() + Path.GetExtension(file.FileName);
            var filePath = Path.Combine(uploadsPath, fileName);

            using (var stream = new FileStream(filePath, FileMode.Create))
            {
                await file.CopyToAsync(stream);
            }

            // URL que será salva no banco
            var url = $"/uploads/artigos/{fileName}";
            return Results.Ok(new { Url = url });
        });
   }
}
