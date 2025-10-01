using Microsoft.EntityFrameworkCore;
using blogger_backend.Models;
using blogger_backend.Data;
using Microsoft.AspNetCore.Mvc;

public static class PesquisaCustomizadaRoute
{
    public static void PesquisaCustomizadaRoutes(this WebApplication app)
    {
        var route = app.MapGroup("/pesquisa");

        //POST 
        route.MapPost("para_escolher", async (PesquisaCustomizadaRequest req, AppDbContext context) =>
        {
            bool jaExiste = await context.PesquisasCustomizadas.AnyAsync(p =>
                p.UsuarioId == req.UsuarioId &&
                p.CategoriaId == req.CategoriaId &&
                p.AutorId == req.AutorId &&
                p.FonteId == req.FonteId
            );

            if (jaExiste)
                return Results.BadRequest(new { Message = "Essa preferência já existe." });

            var pesquisa = new PesquisaCustomizadaModel
            {
                UsuarioId = req.UsuarioId,
                CategoriaId = req.CategoriaId,
                AutorId = req.AutorId,
                FonteId = req.FonteId,
                DataCriacao = DateTime.UtcNow
            };

            await context.PesquisasCustomizadas.AddAsync(pesquisa);
            await context.SaveChangesAsync();

            return Results.Created($"/pesquisa/{pesquisa.Id}", pesquisa);
        });

        //GET - listar todas as preferências de um usuário
        route.MapGet("/{usuarioId:int}preferencia_usuario", async (int usuarioId, AppDbContext context) =>
        {
            var preferencias = await context.PesquisasCustomizadas
                                            .Include(p => p.Categoria)
                                            .Include(p => p.Autor)
                                            .Include(p => p.Fonte)
                                            .Where(p => p.UsuarioId == usuarioId)
                                            .ToListAsync();

            if (!preferencias.Any())
                return Results.NotFound(new { Message = "Nenhuma preferência encontrada." });

            var result = preferencias.Select(p => new
            {
                p.Id,
                Categoria = p.Categoria?.Nome,
                Autor = p.Autor?.Nome,
                Fonte = p.Fonte?.Nome,
                p.DataCriacao
            });

            return Results.Ok(result);
        });

        //PUT - atualizar uma preferência existente
        route.MapPut("/{id:int}para_actualizar", async (int id, PesquisaCustomizadaRequest req, AppDbContext context) =>
        {
            var pesquisa = await context.PesquisasCustomizadas.FirstOrDefaultAsync(p => p.Id == id);
            if (pesquisa == null) return Results.NotFound();

            pesquisa.CategoriaId = req.CategoriaId;
            pesquisa.AutorId = req.AutorId;
            pesquisa.FonteId = req.FonteId;

            await context.SaveChangesAsync();
            return Results.Ok(pesquisa);
        });

        //DELETE - remover preferência
        route.MapDelete("/{id:int}para_remover", async (int id, AppDbContext context) =>
        {
            var pesquisa = await context.PesquisasCustomizadas.FirstOrDefaultAsync(p => p.Id == id);
            if (pesquisa == null) return Results.NotFound();

            context.PesquisasCustomizadas.Remove(pesquisa);
            await context.SaveChangesAsync();

            return Results.Ok(new { Message = "Preferência removida com sucesso." });
        });

        //GET - listar artigos com base nas preferências do usuário na página
        route.MapGet("/artigos/{usuarioId:int}/todos_artigos", async (int usuarioId,[FromQuery] int page, AppDbContext context) =>
        {
            const int pageSize = 3; // fixo, 3 artigos por página

            var preferencias = await context.PesquisasCustomizadas
                                            .Where(p => p.UsuarioId == usuarioId)
                                            .ToListAsync();

            if (!preferencias.Any())
                return Results.NotFound(new { Message = "Nenhuma preferência encontrada para este usuário." });

            List<int> ExtrairIds(Func<PesquisaCustomizadaModel, int?> seletor) =>
                preferencias.Where(p => seletor(p).HasValue)
                            .Select(p => seletor(p)!.Value)
                            .ToList();

            var categoriaIds = ExtrairIds(p => p.CategoriaId);
            var autorIds     = ExtrairIds(p => p.AutorId);
            var fonteIds     = ExtrairIds(p => p.FonteId);

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
                                .Skip((page - 1) * pageSize)
                                .Take(pageSize)
                                .ToListAsync();

            var result = new
            {
                Page = page,
                PageSize = pageSize,
                Total = total,
                TotalPages = (int)Math.Ceiling(total / (double)pageSize),
                Artigos = artigos.Select(a => new
                {
                    a.Id,
                    a.Titulo,
                    a.Imagem,
                    a.Resumo,
                    Autor = a.Autor.Nome,
                    Categoria = a.Categoria.Nome,
                    Fonte = a.Fonte != null ? a.Fonte.Nome : null,
                    a.DataCriacao
                })
            };

            return Results.Ok(result);
      });

      }
}
