using Microsoft.EntityFrameworkCore;
using blogger_backend.Models;
using blogger_backend.Data;

namespace blogger_backend.Routes;

public static class NewsletterRoute
{
    public static void NewsletterRoutes(this WebApplication app)
    {
        var route = app.MapGroup("/newsletter").WithTags("NewsLetters");

        route.MapPost("create", async (NewsletterRequest req, AppDbContext context) =>
        {
            var newsletter = new NewsletterModel
            {
                Email = req.Email,
                Active = req.Active
            };

            await context.Newsletters.AddAsync(newsletter);
            await context.SaveChangesAsync();
            return Results.Created($"/newsletter/{newsletter.Id}", newsletter);
        }).WithSummary("Cadastra Newsletter");

        route.MapGet("show", async (AppDbContext context) =>
        {
            var newsletters = await context.Newsletters
                                           .Where(n => n.Active)
                                           .ToListAsync();
            return Results.Ok(newsletters);
        }).WithSummary("Visualiza Newsletters Ativas");

        route.MapPut("/{id:int}/update", async (int id, NewsletterRequest req, AppDbContext context) =>
        {
            var newsletter = await context.Newsletters.FirstOrDefaultAsync(n => n.Id == id && n.Active);
            if (newsletter == null) return Results.NotFound();

            newsletter.Email = req.Email;
            newsletter.Active = req.Active;
            await context.SaveChangesAsync();
            return Results.Ok(newsletter);
        }).WithSummary("Atualiza Newsletter pelo ID");

        route.MapDelete("/{id:int}/delete", async (int id, AppDbContext context) =>
        {
            var newsletter = await context.Newsletters.FirstOrDefaultAsync(n => n.Id == id && n.Active);
            if (newsletter == null) return Results.NotFound();

            newsletter.Active = false;
            await context.SaveChangesAsync();
            return Results.Ok();
        }).WithSummary("Deleta Newsletter pelo ID");
    }
}
