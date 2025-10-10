using Microsoft.EntityFrameworkCore;
using blogger_backend.Models;
using blogger_backend.Data;

namespace blogger_backend.Routes;

public static class FonteRoute
{
    public static void SourceRoute(this WebApplication app)
    {
        var route = app.MapGroup("/source").WithTags("Sources");;

        route.MapPost("create", async (SourceRequest req, AppDbContext context) =>
        {
            bool exist = await context.Sources.AnyAsync(f =>
                f.Active &&
                (f.Name.ToLower() == req.Name.ToLower() ||
                f.URL.ToLower() == req.URL.ToLower()));

            if (exist)
                return Results.BadRequest(new { Message = "Já existe uma fonte com este nome ou URL." });

            var source = new SourceModel
            {
                Name = req.Name,
                URL = req.URL,
                Type = req.Type,
                Active = true
            };

            await context.Sources.AddAsync(source);
            await context.SaveChangesAsync();
            return Results.Created($"/source/{source.Id}", source);
        });

        route.MapGet("show", async (AppDbContext context) =>
        {
            var source = await context.Sources
                                      .Where(f => f.Active)
                                      .ToListAsync();
            return Results.Ok(source);
        });

        route.MapPut("/{id:int}/update", async (int id, SourceRequest req, AppDbContext context) =>
        {
            var source = await context.Sources.FirstOrDefaultAsync(f => f.Id == id && f.Active);
            if (source == null) return Results.NotFound();
            
            bool again = await context.Sources.AnyAsync(f =>
                f.Id != id &&
                f.Active &&
                (f.Name.ToLower() == req.Name.ToLower() ||
                f.URL.ToLower() == req.URL.ToLower()));

            if (again)
                return Results.BadRequest(new { Message = "Já existe outra fonte com o mesmo nome ou URL." });

            source.Name = req.Name;
            source.URL = req.URL;
            source.Type = req.Type;
            await context.SaveChangesAsync();
            return Results.Ok(source);
        });

        route.MapDelete("/{id:int}/delete", async (int id, AppDbContext context) =>
        {
            var source = await context.Sources.FirstOrDefaultAsync(f => f.Id == id && f.Active);
            if (source == null) return Results.NotFound();

            source.Active = false;
            await context.SaveChangesAsync();
            return Results.Ok();
        });
    }
}
