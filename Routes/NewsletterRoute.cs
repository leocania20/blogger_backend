using Microsoft.EntityFrameworkCore;
using blogger_backend.Models;
using blogger_backend.Data;

namespace blogger_backend.Routes;

public static class NewsletterRoute
{
    public static void NewsletterRoutes(this WebApplication app)
    {
        var route = app.MapGroup("/newsletter");

        // POST
        route.MapPost("", async (NewsletterRequest req, AppDbContext context) =>
        {
            var newsletter = new NewsletterModel
            {
                Email = req.Email,
                Ativo = req.Ativo
            };

            await context.Newsletters.AddAsync(newsletter);
            await context.SaveChangesAsync();
            return Results.Created($"/newsletter/{newsletter.Id}", newsletter);
        });

        // GET
        route.MapGet("", async (AppDbContext context) =>
        {
            var newsletters = await context.Newsletters
                                           .Where(n => n.Ativo)
                                           .ToListAsync();
            return Results.Ok(newsletters);
        });

        // PUT
        route.MapPut("/{id:int}", async (int id, NewsletterRequest req, AppDbContext context) =>
        {
            var newsletter = await context.Newsletters.FirstOrDefaultAsync(n => n.Id == id && n.Ativo);
            if (newsletter == null) return Results.NotFound();

            newsletter.Email = req.Email;
            newsletter.Ativo = req.Ativo;
            await context.SaveChangesAsync();
            return Results.Ok(newsletter);
        });

        // DELETE 
        route.MapDelete("/{id:int}", async (int id, AppDbContext context) =>
        {
            var newsletter = await context.Newsletters.FirstOrDefaultAsync(n => n.Id == id && n.Ativo);
            if (newsletter == null) return Results.NotFound();

            newsletter.Ativo = false;
            await context.SaveChangesAsync();
            return Results.Ok();
        });
    }
}
