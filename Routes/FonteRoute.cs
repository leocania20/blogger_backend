using Microsoft.EntityFrameworkCore;
using blogger_backend.Models;
using blogger_backend.Data;

namespace blogger_backend.Routes;

public static class FonteRoute
{
    public static void FonteRoutes(this WebApplication app)
    {
        var route = app.MapGroup("/fonte");

        // POST
        route.MapPost("", async (FonteRequest req, AppDbContext context) =>
        {
            var fonte = new FonteModel
            {
                Nome = req.Nome,
                URL = req.URL,
                Tipo = req.Tipo,
                Ativo = true
            };

            await context.Fontes.AddAsync(fonte);
            await context.SaveChangesAsync();
            return Results.Created($"/fonte/{fonte.Id}", fonte);
        });

        // GET
        route.MapGet("", async (AppDbContext context) =>
        {
            var fontes = await context.Fontes
                                      .Where(f => f.Ativo)
                                      .ToListAsync();
            return Results.Ok(fontes);
        });

        // PUT
        route.MapPut("/{id:int}", async (int id, FonteRequest req, AppDbContext context) =>
        {
            var fonte = await context.Fontes.FirstOrDefaultAsync(f => f.Id == id && f.Ativo);
            if (fonte == null) return Results.NotFound();

            fonte.Nome = req.Nome;
            fonte.URL = req.URL;
            fonte.Tipo = req.Tipo;
            await context.SaveChangesAsync();
            return Results.Ok(fonte);
        });

        // DELETE 
        route.MapDelete("/{id:int}", async (int id, AppDbContext context) =>
        {
            var fonte = await context.Fontes.FirstOrDefaultAsync(f => f.Id == id && f.Ativo);
            if (fonte == null) return Results.NotFound();

            fonte.Ativo = false;
            await context.SaveChangesAsync();
            return Results.Ok();
        });
    }
}
