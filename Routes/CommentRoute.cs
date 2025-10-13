using Microsoft.EntityFrameworkCore;
using blogger_backend.Models;
using blogger_backend.Data;
using blogger_backend.Utils;

namespace blogger_backend.Routes;

public static class ComentarioRoute
{
    public static void CommentRoutes(this WebApplication app)
    {
        var route = app.MapGroup("/comment").WithTags("Comments");

        route.MapPost("create", async (CommentRequest req, AppDbContext context) =>
        {
            var errors = ValidationHelper.ValidateModel(req);
            if (errors.Any())
                return Results.BadRequest(new { Errors = errors });

            var comment = new CommentModel
            {
                Text = req.Text,
                Status = req.Status,
                UserId = req.UserId,
                ArticleId = req.ArticleId,
                Active = req.Active,
                CreateDate = DateTime.UtcNow
            };

            await context.Comments.AddAsync(comment);
            await context.SaveChangesAsync();

            return Results.Created($"/comment/{comment.Id}", comment);
        });

        route.MapPut("/{id:int}/update", async (int id, CommentRequest req, AppDbContext context) =>
        {
            var comment = await context.Comments.FirstOrDefaultAsync(c => c.Id == id && c.Active);
            if (comment == null)
                return Results.NotFound(new { Error = "Comentário não encontrado." });

            var errors = ValidationHelper.ValidateModel(req);
            if (errors.Any())
                return Results.BadRequest(new { Errors = errors });

            comment.Text = req.Text;
            comment.Status = req.Status;
            comment.ArticleId = req.ArticleId;
            comment.UserId = req.UserId;
            comment.Active = req.Active;

            await context.SaveChangesAsync();
            return Results.Ok(comment);
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
