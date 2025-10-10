using Microsoft.EntityFrameworkCore;
using blogger_backend.Models;
using blogger_backend.Data;

namespace blogger_backend.Routes;

public static class AutorRoute
{
    public static void AuthorRoutes(this WebApplication app)
    {
        var route = app.MapGroup("/author").WithTags("Authores");

        route.MapPost("create", async (AuthorRequest req, AppDbContext context) =>
        {
             bool exist = await context.Authores.AnyAsync(a =>
                a.Active &&
                (a.Email.ToLower() == req.Email.ToLower() ||
                a.Name.ToLower() == req.Name.ToLower()));

            if (exist)
                return Results.BadRequest(new { Message = "Já existe um autor com este nome ou e-mail." });

            var author = new AuthorModel
            {
                Name = req.Name,
                Bio = req.Bio,
                Email = req.Email,
                UserId = req.UserId,
                Active = true
            };

            await context.Authores.AddAsync(author);
            await context.SaveChangesAsync();
            return Results.Created($"/author/{author.Id}", author);
        });

        route.MapGet("show", async (AppDbContext context) =>
        {
            var autores = await context.Authores
                                      .Where(a => a.Active)
                                      .ToListAsync();
            return Results.Ok(autores);
        });

        route.MapPut("/{id:int}/update", async (int id, AuthorRequest req, AppDbContext context) =>
        {
            var author = await context.Authores.FirstOrDefaultAsync(a => a.Id == id && a.Active);
            if (author == null) return Results.NotFound();

            bool again = await context.Authores.AnyAsync(a =>
                a.Id != id &&
                a.Active &&
                (a.Email.ToLower() == req.Email.ToLower() ||
                a.Name.ToLower() == req.Name.ToLower()));

            if (again)
                return Results.BadRequest(new { Message = "Outro autor com o mesmo nome ou e-mail já existe." });

            author.Name = req.Name;
            author.Bio = req.Bio;
            author.Email = req.Email;
            author.UserId = req.UserId;

            await context.SaveChangesAsync();
            return Results.Ok(author);
        });

        route.MapDelete("/{id:int}/delete", async (int id, AppDbContext context) =>
        {
            var author = await context.Authores.FirstOrDefaultAsync(a => a.Id == id && a.Active);
            if (author == null) return Results.NotFound();

            author.Active = false; 
            await context.SaveChangesAsync();
            return Results.Ok();
        });
    }
}
