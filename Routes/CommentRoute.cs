using Microsoft.EntityFrameworkCore;
using blogger_backend.Models;
using blogger_backend.Data;

namespace blogger_backend.Routes;

public static class ComentarioRoute
{
    public static void CommentRoutes(this WebApplication app)
    {
        var route = app.MapGroup("/comment").WithTags("Comments");

        route.MapPost("create", async (CommentRequest req, AppDbContext context) =>
        {
            var comment= new CommentModel
            {
                Text = req.Text,
                Status = req.Status,
                UserId = req.UserId,
                ArticleId = req.ArticleId,
                Active = true,
                CreateDate = DateTime.UtcNow
            };

            await context.Comments.AddAsync(comment);
            await context.SaveChangesAsync();
            return Results.Created($"/comentario/{comment.Id}", comment);
        });

        route.MapGet("show", async (AppDbContext context) =>
        {
            var comments = await context.Comments
                                           .Where(c => c.Active)
                                           .Include(c => c.User)
                                           .Include(c => c.Article)
                                           .ToListAsync();
            return Results.Ok(comments);
        });

        route.MapPut("/{id:int}/update", async (int id, CommentRequest req, AppDbContext context) =>
        {
            var comment = await context.Comments.FirstOrDefaultAsync(c => c.Id == id && c.Active);
            if (comment == null) return Results.NotFound();

            comment.Text = req.Text;
            comment.Status = req.Status;
            await context.SaveChangesAsync();
            return Results.Ok(comment);
        });

        route.MapDelete("/{id:int}/delete", async (int id, AppDbContext context) =>
        {
            var comment = await context.Comments.FirstOrDefaultAsync(c => c.Id == id && c.Active);
            if (comment == null) return Results.NotFound();

            comment.Active = false; 
            await context.SaveChangesAsync();
            return Results.Ok();
        });
    }
}
