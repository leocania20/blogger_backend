using Microsoft.EntityFrameworkCore;
using blogger_backend.Models;
using blogger_backend.Data;
using blogger_backend.Utils;

namespace blogger_backend.Routes;

public static class AutorRoute
{
    public static void AuthorRoutes(this WebApplication app)
    {
        var route = app.MapGroup("/author").WithTags("Authores");

        route.MapPost("create", async (AuthorRequest req, AppDbContext context) =>
        {
            var error = ValidationHelper.ValidateModel(req);
            if (error.Any())
                return Results.BadRequest(new { Errors = error });

            bool exists = await context.Authores.AnyAsync(a =>
                a.Active &&
                (a.Email.ToLower() == req.Email.ToLower() ||
                 a.Name.ToLower() == req.Name.ToLower()));

            if (exists)
                return Results.BadRequest(new { Error = "Já existe um autor com este nome ou e-mail." });

            var author = new AuthorModel
            {
                Name = req.Name.Trim(),
                Bio = req.Bio.Trim(),
                Email = req.Email.Trim().ToLower(),
                UserId = req.UserId,
                Active = true
            };

            await context.Authores.AddAsync(author);
            await context.SaveChangesAsync();

            return Results.Created($"/author/{author.Id}", new
            {
                author.Id,
                author.Name,
                author.Email,
                author.Bio,
                author.UserId,
                author.Active
            });
        });

        route.MapPut("/{id:int}/update", async (int id, AuthorRequest req, AppDbContext context) =>
        {
            var author = await context.Authores.FirstOrDefaultAsync(a => a.Id == id && a.Active);
            if (author == null)
                return Results.NotFound(new { Error = "Autor não encontrado." });

            var error = ValidationHelper.ValidateModel(req);
            if (error.Any())
                return Results.BadRequest(new { Errors = error });

            bool duplicate = await context.Authores.AnyAsync(a =>
                a.Id != id &&
                a.Active &&
                (a.Email.ToLower() == req.Email.ToLower() ||
                 a.Name.ToLower() == req.Name.ToLower()));

            if (duplicate)
                return Results.BadRequest(new { Error = "Outro autor com o mesmo nome ou e-mail já existe." });

            author.Name = req.Name.Trim();
            author.Bio = req.Bio.Trim();
            author.Email = req.Email.Trim().ToLower();
            author.UserId = req.UserId;

            await context.SaveChangesAsync();

            return Results.Ok(new
            {
                author.Id,
                author.Name,
                author.Email,
                author.Bio,
                author.UserId,
                author.Active
            });
        });
    
        route.MapGet("show", async (AppDbContext context) =>
        {
            var autores = await context.Authores
                                      .Where(a => a.Active)
                                      .ToListAsync();
            return Results.Ok(autores);
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
