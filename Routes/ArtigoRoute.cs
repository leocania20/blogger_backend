using System.Numerics;
using blogger_backend.Data;
using blogger_backend.Models;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;

namespace blogger_backend.Routes;

public static class ArtigoRoute
{
    public static void ArtigoRoutes(this WebApplication app)
    {
        var route = app.MapGroup("/artigo");

        // post
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

        // get
        route.MapGet("completo", async (
            int? id,
            AppDbContext context) =>
        {
            var query = context.Artigos
                               .Include(a => a.Categoria)
                               .Include(a => a.Autor)
                               .Include(a => a.Fonte)
                               .Where(a => a.IsPublicado)
                               .AsQueryable();

            if (id.HasValue)
                query = query.Where(a => a.Id == id.Value);

            var artigos = await query
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

        // get
        route.MapGet("/paginaInicial", async (int? page, AppDbContext context) =>
        {
            int pageSize = 6;
            int currentPage = page ?? 1;
            if (currentPage < 1) currentPage = 1;

            var query = context.Artigos
                .Include(a => a.Categoria)
                .Include(a => a.Autor)
                .Include(a => a.Fonte)
                .Where(a => a.IsPublicado);

            var artigosBase = await query
                .OrderByDescending(a => a.DataCriacao)
                .Skip((currentPage - 1) * pageSize)
                .Take(pageSize)
                .ToListAsync();

            var verificationFonte = new blogger_backend.Utils.VerificationFonte(context);

            var artigos = new List<object>();

            foreach (var artigo in artigosBase)
            {
                var urlFonte = await verificationFonte.ObterUrlFontePorArtigoAsync(artigo.Id) ?? "";

                artigos.Add(new
                {
                    artigo.Id,
                    artigo.Titulo,
                    artigo.Imagem,
                    artigo.DataCriacao,
                    Autor = artigo.Autor.Nome,
                    artigo.Resumo,
                    UrlFonte = urlFonte 
                });
            }

            var total = await query.CountAsync();

            return Results.Ok(new
            {
                Pagina = currentPage,
                PageSize = pageSize,
                Total = total,
                TotalPaginas = (int)Math.Ceiling(total / (double)pageSize),
                Artigos = artigos
            });
        });

        // put
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

        //del
        route.MapDelete("/{id:int}", async (int id, AppDbContext context) =>
        {
            var artigo = await context.Artigos.FirstOrDefaultAsync(a => a.Id == id);
            if (artigo == null) return Results.NotFound();

            artigo.IsPublicado = false;
            await context.SaveChangesAsync();
            return Results.Ok();
        });

        // search
        route.MapGet("/pesquisar_geral", async (
            string? titulo,
            [FromQuery] int[]? categorias,
            [FromQuery] int[]? fontes,
            DateTime? data,
            int? page,
            AppDbContext context) =>
        {
            int pageSize = 6;
            int currentPage = page ?? 1;
            
            if (currentPage < 1) currentPage = 1;

            var query = context.Artigos
                .Include(a => a.Categoria)
                .Include(a => a.Autor)
                .Include(a => a.Fonte)
                .Where(a => a.IsPublicado)
                .AsQueryable();

            if (string.IsNullOrWhiteSpace(titulo) &&
                (categorias == null || !categorias.Any()) &&
                (fontes == null || !fontes.Any()) &&
                !data.HasValue)
            {
                return Results.Ok(await query
                    .OrderByDescending(a => a.DataCriacao)
                    .Skip((currentPage - 1) * pageSize)
                    .Take(pageSize)
                    .Select(a => new
                    {
                        a.Titulo,
                        a.Imagem,
                        a.DataCriacao,
                        Autor = a.Autor.Nome,
                        a.Resumo
                    }).ToListAsync());
            }

            query = query.Where(a =>
                (!string.IsNullOrWhiteSpace(titulo) && a.Titulo.ToLower().Contains(titulo.ToLower())) ||
                (categorias != null && categorias.Contains(a.CategoriaId)) ||
                (fontes != null && a.FonteId != null && fontes.Contains(a.FonteId.Value)) ||
                (data.HasValue && a.DataCriacao.Date == data.Value.Date)
            );

            var artigos = await query
                .OrderByDescending(a => a.DataCriacao)
                .Skip((currentPage - 1) * pageSize)
                .Take(pageSize)
                .Select(a => new
                {
                    a.Titulo,
                    a.Imagem,
                    a.DataCriacao,
                    Autor = a.Autor.Nome,
                    a.Resumo
                })
                .ToListAsync();

            return Results.Ok(new
            {
                Pagina = page,
                Total = await query.CountAsync(),
                Artigos = artigos
            });
        });

        // upload img 
        route.MapPost("/upload-imagem", async (HttpRequest request, IConfiguration config) =>
        {
            var form = await request.ReadFormAsync();
            var file = form.Files.FirstOrDefault();

            if (file == null || file.Length == 0)
                return Results.BadRequest("Nenhuma imagem enviada.");

            var uploadsPath = Path.Combine(Directory.GetCurrentDirectory(), "wwwroot", "uploads", "artigos");
            if (!Directory.Exists(uploadsPath))
                Directory.CreateDirectory(uploadsPath);

            var fileName = Guid.NewGuid().ToString() + Path.GetExtension(file.FileName);
            var filePath = Path.Combine(uploadsPath, fileName);

            using (var stream = new FileStream(filePath, FileMode.Create))
            {
                await file.CopyToAsync(stream);
            }


            var baseUrl = config.GetValue<string>("BackendSettings:BaseUrl");

            if (string.IsNullOrWhiteSpace(baseUrl))
            {
                return Results.Ok(new { Url = $"/uploads/artigos/{fileName}" }); 
            }
            var url = $"{baseUrl.TrimEnd('/')}/uploads/artigos/{fileName}";
            return Results.Ok(new { Url = url });
        });
    }
}
