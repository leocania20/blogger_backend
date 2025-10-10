using Microsoft.EntityFrameworkCore;
using blogger_backend.Models;
using blogger_backend.Data;

namespace blogger_backend.Routes;

public static class notificationRoute
{
    public static void NotificationRoutes(this WebApplication app)
    {
        var route = app.MapGroup("/notification").WithTags("Notifications");

        route.MapPost("create", async (NotificationRequest req, AppDbContext context) =>
        {
            var notification = new NotificationModel
            {
                Title = req.Title,
                Message = req.Message,
                Type = req.Type,
                UserId = req.UserId,
                ArticleId = req.ArticleId,
                Readed = req.Readed,
                Active = true,
                CreateDate = DateTime.UtcNow
            };

            await context.Notifications.AddAsync(notification);
            await context.SaveChangesAsync();
            return Results.Created($"/notification/{notification.Id}", notification);
        });

        route.MapGet("show", async (AppDbContext context) =>
        {
            var notification = await context.Notifications
                                            .Where(n => n.Active)
                                            .Include(n => n.User)
                                            .Include(n => n.Article)
                                            .ToListAsync();
            return Results.Ok(notification);
        });

        route.MapPut("/{id:int}/update", async (int id, NotificationRequest req, AppDbContext context) =>
        {
            var notification = await context.Notifications.FirstOrDefaultAsync(n => n.Id == id && n.Active);
            if (notification == null) return Results.NotFound();

            notification.Title = req.Title;
            notification.Message = req.Message;
            notification.Type = req.Type;
            notification.Readed = req.Readed;

            await context.SaveChangesAsync();
            return Results.Ok(notification);
        });

        route.MapDelete("/{id:int}/delete", async (int id, AppDbContext context) =>
        {
            var notification = await context.Notifications.FirstOrDefaultAsync(n => n.Id == id && n.Active);
            if (notification == null) return Results.NotFound();

            notification.Active = false; 
            await context.SaveChangesAsync();
            return Results.Ok();
        });
    }
}
