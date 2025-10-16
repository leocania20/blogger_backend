using Microsoft.EntityFrameworkCore;
using blogger_backend.Models;
using blogger_backend.Data;
using System.Security.Claims;

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
        }).WithSummary("Criar Notificação");

        route.MapGet("show", async (AppDbContext context) =>
        {
            var notifications = await context.Notifications
            .Where(n => n.Active)
            .Include(n => n.User)
            .Include(n => n.Article)
            .Select(n => new
            {
                n.Id,
                n.Title,
                n.Message,
                n.Type,
                n.Readed,
                n.CreateDate,
                User = n.User == null ? null : new
                {
                    Id = n.User.Id,
                    Name = n.User.Name,
                    Email = n.User.Email
                },
                Article = n.Article == null ? null : new
                {
                    Id = n.Article.Id,
                    Title = n.Article.Title
                }
            })
            .ToListAsync();

            return Results.Ok(notifications);
        }).WithSummary("Visualizar Notificações Ativas");

        route.MapGet("/my-notifications", async (HttpContext http, AppDbContext context) =>
        {
            var userId = http.User.FindFirstValue("id");
            if (string.IsNullOrEmpty(userId))
                return Results.Unauthorized();

            int UserId = int.Parse(userId);
            var notifications = await context.Notifications
                .Where(n => n.Active && n.UserId == UserId)
                .Include(n => n.User)
                .Include(n => n.Article)
                .Select(n => new {
                    n.Id,
                    n.Title,
                    n.Message,
                    n.Type,
                    n.Readed,
                    n.CreateDate,
                    User = new {
                        Id = n.User!.Id,
                        Name = n.User.Name,
                        Email = n.User.Email
                    },
                    Article = n.Article == null ? null : new {
                        Id = n.Article.Id,
                        Title = n.Article.Title
                    }
                })
                .OrderByDescending(n => n.CreateDate)
                .ToListAsync();

            return Results.Ok(notifications);
        }).WithSummary("Visualizar Notificações Ativas do Usuário Logado")
          .RequireAuthorization();


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
        }).WithSummary("Atualizar Notificação pelo ID");

        route.MapDelete("/{id:int}/delete", async (int id, AppDbContext context) =>
        {
            var notification = await context.Notifications.FirstOrDefaultAsync(n => n.Id == id && n.Active);
            if (notification == null) return Results.NotFound();

            notification.Active = false;
            await context.SaveChangesAsync();
            return Results.Ok();
        }).WithSummary("Desativar Notificação pelo ID")
            .RequireAuthorization();
    }
}
