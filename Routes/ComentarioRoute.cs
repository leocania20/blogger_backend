using Microsoft.EntityFrameworkCore;
using blogger_backend.Models;
using blogger_backend.Data;

namespace blogger_backend.Routes;

public static class ComentarioRoute
{
    public static void ComentarioRoutes(this WebApplication app)
    {
        var route = app.MapGroup("/comentario");

        // POST
        route.MapPost("", async (ComentarioRequest req, AppDbContext context) =>
        {
            var comentario = new ComentarioModel
            {
                Conteudo = req.Conteudo,
                Status = req.Status,
                UsuarioId = req.UsuarioId,
                ArtigoId = req.ArtigoId,
                Ativo = true,
                DataCriacao = DateTime.UtcNow
            };

            await context.Comentarios.AddAsync(comentario);
            await context.SaveChangesAsync();
            return Results.Created($"/comentario/{comentario.Id}", comentario);
        });

        // GET
        route.MapGet("", async (AppDbContext context) =>
        {
            var comentarios = await context.Comentarios
                                           .Where(c => c.Ativo)
                                           .Include(c => c.Usuario)
                                           .Include(c => c.Artigo)
                                           .ToListAsync();
            return Results.Ok(comentarios);
        });

        // PUT
        route.MapPut("/{id:int}", async (int id, ComentarioRequest req, AppDbContext context) =>
        {
            var comentario = await context.Comentarios.FirstOrDefaultAsync(c => c.Id == id && c.Ativo);
            if (comentario == null) return Results.NotFound();

            comentario.Conteudo = req.Conteudo;
            comentario.Status = req.Status;
            await context.SaveChangesAsync();
            return Results.Ok(comentario);
        });

        // DELETE 
        route.MapDelete("/{id:int}", async (int id, AppDbContext context) =>
        {
            var comentario = await context.Comentarios.FirstOrDefaultAsync(c => c.Id == id && c.Ativo);
            if (comentario == null) return Results.NotFound();

            comentario.Ativo = false; // Soft delete
            await context.SaveChangesAsync();
            return Results.Ok();
        });
    }
}
