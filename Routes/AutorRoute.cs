using Microsoft.EntityFrameworkCore;
using blogger_backend.Models;
using blogger_backend.Data;

namespace blogger_backend.Routes;

public static class AutorRoute
{
    public static void AutorRoutes(this WebApplication app)
    {
        var route = app.MapGroup("/autor");

        // POST
        route.MapPost("", async (AutorRequest req, AppDbContext context) =>
        {
            var autor = new AutorModel
            {
                Nome = req.Nome,
                Bio = req.Bio,
                Email = req.Email,
                UsuarioId = req.UsuarioId,
                Ativo = true
            };

            await context.Autores.AddAsync(autor);
            await context.SaveChangesAsync();
            return Results.Created($"/autor/{autor.Id}", autor);
        });

        // GET
        route.MapGet("", async (AppDbContext context) =>
        {
            var autores = await context.Autores
                                      .Where(a => a.Ativo)
                                      .ToListAsync();
            return Results.Ok(autores);
        });

        // PUT
        route.MapPut("/{id:int}", async (int id, AutorRequest req, AppDbContext context) =>
        {
            var autor = await context.Autores.FirstOrDefaultAsync(a => a.Id == id && a.Ativo);
            if (autor == null) return Results.NotFound();

            autor.Nome = req.Nome;
            autor.Bio = req.Bio;
            autor.Email = req.Email;
            autor.UsuarioId = req.UsuarioId;

            await context.SaveChangesAsync();
            return Results.Ok(autor);
        });

        // DELETE 
        route.MapDelete("/{id:int}", async (int id, AppDbContext context) =>
        {
            var autor = await context.Autores.FirstOrDefaultAsync(a => a.Id == id && a.Ativo);
            if (autor == null) return Results.NotFound();

            autor.Ativo = false; 
            await context.SaveChangesAsync();
            return Results.Ok();
        });
    }
}
