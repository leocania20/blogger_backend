using Microsoft.EntityFrameworkCore;
using blogger_backend.Models;
using blogger_backend.Data;

namespace blogger_backend.Routes;

public static class NotificacaoRoute
{
    public static void NotificacaoRoutes(this WebApplication app)
    {
        var route = app.MapGroup("/notificacao");

        // POST
        route.MapPost("", async (NotificacaoRequest req, AppDbContext context) =>
        {
            var notificacao = new NotificacaoModel
            {
                Titulo = req.Titulo,
                Mensagem = req.Mensagem,
                Tipo = req.Tipo,
                UsuarioId = req.UsuarioId,
                ArtigoId = req.ArtigoId,
                Lida = req.Lida,
                Ativo = true,
                DataCriacao = DateTime.UtcNow
            };

            await context.Notificacoes.AddAsync(notificacao);
            await context.SaveChangesAsync();
            return Results.Created($"/notificacao/{notificacao.Id}", notificacao);
        });

        // GET
        route.MapGet("", async (AppDbContext context) =>
        {
            var notificacoes = await context.Notificacoes
                                            .Where(n => n.Ativo)
                                            .Include(n => n.Usuario)
                                            .Include(n => n.Artigo)
                                            .ToListAsync();
            return Results.Ok(notificacoes);
        });

        // PUT
        route.MapPut("/{id:int}", async (int id, NotificacaoRequest req, AppDbContext context) =>
        {
            var notificacao = await context.Notificacoes.FirstOrDefaultAsync(n => n.Id == id && n.Ativo);
            if (notificacao == null) return Results.NotFound();

            notificacao.Titulo = req.Titulo;
            notificacao.Mensagem = req.Mensagem;
            notificacao.Tipo = req.Tipo;
            notificacao.Lida = req.Lida;

            await context.SaveChangesAsync();
            return Results.Ok(notificacao);
        });

        // DELETE (soft delete)
        route.MapDelete("/{id:int}", async (int id, AppDbContext context) =>
        {
            var notificacao = await context.Notificacoes.FirstOrDefaultAsync(n => n.Id == id && n.Ativo);
            if (notificacao == null) return Results.NotFound();

            notificacao.Ativo = false; // Soft delete
            await context.SaveChangesAsync();
            return Results.Ok();
        });
    }
}
