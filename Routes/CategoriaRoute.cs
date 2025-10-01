using Microsoft.EntityFrameworkCore;
using blogger_backend.Models;
using blogger_backend.Data;

namespace blogger_backend.Routes;

public static class CategoriaRoute
{
    public static void CategoriaRoutes(this WebApplication app)
    {
        var route = app.MapGroup("/categoria");

        // POST
        route.MapPost("", async (CategoriaRequest req, AppDbContext context) =>
        {
            var categoria = new CategoriaModel
            {
                Nome = req.Nome,
                Descricao = req.Descricao,
                Slug = req.Slug,
                Ativo = req.Ativo
            };

            await context.Categorias.AddAsync(categoria);
            await context.SaveChangesAsync();
            return Results.Created($"/categoria/{categoria.Id}", categoria);
        });

        // GET
        route.MapGet("", async (AppDbContext context) =>
        {
            var categorias = await context.Categorias
                                          .Where(c => c.Ativo)
                                          .ToListAsync();
            return Results.Ok(categorias);
        });

        // PUT
        route.MapPut("/{id:int}", async (int id, CategoriaRequest req, AppDbContext context) =>
        {
            var categoria = await context.Categorias.FirstOrDefaultAsync(c => c.Id == id && c.Ativo);
            if (categoria == null) return Results.NotFound();

            categoria.Nome = req.Nome;
            categoria.Descricao = req.Descricao;
            categoria.Slug = req.Slug;
            categoria.Ativo = req.Ativo;

            await context.SaveChangesAsync();
            return Results.Ok(categoria);
        });

        // DELETE 
        route.MapDelete("/{id:int}", async (int id, AppDbContext context) =>
        {
            var categoria = await context.Categorias.FirstOrDefaultAsync(c => c.Id == id && c.Ativo);
            if (categoria == null) return Results.NotFound();

            categoria.Ativo = false; // Soft delete
            await context.SaveChangesAsync();
            return Results.Ok();
        });
    }
}
