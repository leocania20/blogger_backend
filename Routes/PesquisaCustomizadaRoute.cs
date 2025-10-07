using Microsoft.EntityFrameworkCore;
using blogger_backend.Models;
using blogger_backend.Data;
using Microsoft.AspNetCore.Mvc;
using System.Security.Claims;

public static class PesquisaCustomizadaRoute
{
    public static void PesquisaCustomizadaRoutes(this WebApplication app)
    {
        var route = app.MapGroup("/pesquisa").RequireAuthorization();

        // POST
        route.MapPost("setings", async (PesquisaCustomizadaRequest req, AppDbContext context, HttpContext http) =>
        {
            var userId = http.User.FindFirst("id")?.Value;
            if (string.IsNullOrEmpty(userId))
                return Results.Unauthorized();

            int usuarioId = int.Parse(userId);

            bool jaExiste = await context.PesquisasCustomizadas.AnyAsync(p =>
                p.UsuarioId == usuarioId &&
                p.CategoriaId == req.CategoriaId &&
                p.AutorId == req.AutorId &&
                p.FonteId == req.FonteId
            );

            if (jaExiste)
                return Results.BadRequest(new { Message = "Essa combinação de categoria, autor e fonte já foi salva para este usuário." });

            var pesquisa = new PesquisaCustomizadaModel
            {
                UsuarioId = usuarioId,
                CategoriaId = req.CategoriaId,
                AutorId = req.AutorId,
                FonteId = req.FonteId,
                DataCriacao = DateTime.UtcNow
            };

            try
            {
                await context.PesquisasCustomizadas.AddAsync(pesquisa);
                await context.SaveChangesAsync();
            }
            catch (DbUpdateException)
            {
                return Results.BadRequest(new { Message = "Registro duplicado detectado. Essa preferência já existe." });
            }

            return Results.Created($"/pesquisa/{pesquisa.Id}", new
            {
                pesquisa.Id,
                pesquisa.UsuarioId,
                pesquisa.CategoriaId,
                pesquisa.AutorId,
                pesquisa.FonteId,
                pesquisa.DataCriacao
            });
        });

        // GET - listar todas as preferências do usuário logado
        route.MapGet("preferencia_usuario", async (HttpContext http, AppDbContext context) =>
         {
             var userId = http.User.FindFirstValue("id");
             if (string.IsNullOrEmpty(userId)) return Results.Unauthorized();

             int usuarioId = int.Parse(userId);

             var preferencias = await context.PesquisasCustomizadas
                                             .Include(p => p.Categoria)
                                             .Include(p => p.Autor)
                                             .Include(p => p.Fonte)
                                             .Where(p => p.UsuarioId == usuarioId)
                                             .ToListAsync();

             if (!preferencias.Any())
                 return Results.NotFound(new { Message = "Nenhuma preferência encontrada." });

             var categorias = preferencias
                 .Where(p => p.Categoria != null)
                 .Select(p => new { p.CategoriaId, Nome = p.Categoria!.Nome })
                 .Distinct()
                 .ToList();

             var autores = preferencias
                 .Where(p => p.Autor != null)
                 .Select(p => new { p.AutorId, Nome = p.Autor!.Nome })
                 .Distinct()
                 .ToList();

             var fontes = preferencias
                 .Where(p => p.Fonte != null)
                 .Select(p => new { p.FonteId, Nome = p.Fonte!.Nome })
                 .Distinct()
                 .ToList();

             var result = new
             {
                 Categorias = categorias,
                 Autores = autores,
                 Fontes = fontes
             };

             return Results.Ok(result);
         });

        // DELETE 
        route.MapDelete("deletar_preferencia", async ([FromQuery] int? categoriaId, [FromQuery] int? autorId, [FromQuery] int? fonteId, HttpContext http, AppDbContext context) =>
        {
            var userId = http.User.FindFirstValue("id");
            if (string.IsNullOrEmpty(userId)) return Results.Unauthorized();
            int usuarioId = int.Parse(userId);

            var pesquisas = await context.PesquisasCustomizadas
                                         .Where(p => p.UsuarioId == usuarioId &&
                                                ((categoriaId.HasValue && p.CategoriaId == categoriaId) ||
                                                  (autorId.HasValue && p.AutorId == autorId) ||
                                                  (fonteId.HasValue && p.FonteId == fonteId)))
                                         .ToListAsync();

            if (!pesquisas.Any())
                return Results.NotFound(new { Message = "Nenhuma preferência encontrada para os filtros informados." });

            context.PesquisasCustomizadas.RemoveRange(pesquisas);
            await context.SaveChangesAsync();

            return Results.Ok(new { Message = "Preferência(s) deletada(s) com sucesso." });
        });
        
        // GET - listar artigos de acordo com preferências do usuário
        route.MapGet("artigos/preferencias", async (
            HttpContext http,
            AppDbContext context,
            [FromQuery] int? page) =>
        {
            var userId = http.User.FindFirstValue("id");
            if (string.IsNullOrEmpty(userId)) return Results.Unauthorized();
            int usuarioId = int.Parse(userId);

            int pageSize = 3;                 
            int currentPage = page ?? 1;      
            if (currentPage < 1) currentPage = 1;

            var preferencias = await context.PesquisasCustomizadas
                                            .Where(p => p.UsuarioId == usuarioId)
                                            .ToListAsync();

            if (!preferencias.Any())
                return Results.NotFound(new { Message = "Nenhuma preferência encontrada para este usuário." });

            var categoriaIds = preferencias.Where(p => p.CategoriaId.HasValue).Select(p => p.CategoriaId!.Value).ToList();
            var autorIds     = preferencias.Where(p => p.AutorId.HasValue).Select(p => p.AutorId!.Value).ToList();
            var fonteIds     = preferencias.Where(p => p.FonteId.HasValue).Select(p => p.FonteId!.Value).ToList();

            var query = context.Artigos
                            .Include(a => a.Categoria)
                            .Include(a => a.Autor)
                            .Include(a => a.Fonte)
                            .Where(a => a.IsPublicado &&
                                        (categoriaIds.Contains(a.CategoriaId) ||
                                        autorIds.Contains(a.AutorId) ||
                                        (a.FonteId.HasValue && fonteIds.Contains(a.FonteId.Value))));

            var total = await query.CountAsync();

            var artigos = await query
                                .OrderByDescending(a => a.DataCriacao)
                                .Skip((currentPage - 1) * pageSize)
                                .Take(pageSize)
                                .Select(a => new
                                {
                                    a.Id,
                                    a.Titulo,
                                    a.Imagem,
                                    a.DataCriacao,
                                    Autor = a.Autor.Nome,
                                    a.Resumo
                                })
                                .ToListAsync();

            return Results.Ok(new
            {
                Pagina = currentPage,
                PageSize = pageSize,
                TotalArtigos = total,
                TotalPaginas = (int)Math.Ceiling(total / (double)pageSize),
                Artigos = artigos
            });
        }); 

    }
}
