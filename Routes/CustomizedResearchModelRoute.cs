using Microsoft.EntityFrameworkCore;
using blogger_backend.Models;
using blogger_backend.Data;
using Microsoft.AspNetCore.Mvc;
using System.Security.Claims;

public static class CustomizedResearchRoute
{
    public static void PesquisaCustomizadaRoutes(this WebApplication app)
    {
        var route = app.MapGroup("/seetigns").WithTags("Seetings").RequireAuthorization(); 

        route.MapPost("up", async (CustomizedResearchRequest req, AppDbContext context, HttpContext http) =>
        {
            var userId = http.User.FindFirst("id")?.Value;
            if (string.IsNullOrEmpty(userId))
                return Results.Unauthorized();

            int usersId = int.Parse(userId);

            bool alreadyExist = await context.CustomizedResearches.AnyAsync(c=>
                c.UserId == usersId &&
                c.CategoryId == req.CategoryId &&
                c.AuthorId == req.AuthorId &&
                c.SourceId == req.SourceId
            );

            if (alreadyExist)
                return Results.BadRequest(new { Message = "Essa combinação de categoria, autor e fonte já foi salva para este usuário." });

            var seeting = new CustomizedResearchModel
            {
                UserId = usersId,
                CategoryId = req.CategoryId,
                AuthorId = req.AuthorId,
                SourceId = req.SourceId,
                CreateDate = DateTime.UtcNow
            };

            try
            {
                await context.CustomizedResearches.AddAsync(seeting);
                await context.SaveChangesAsync();
            }
            catch (DbUpdateException)
            {
                return Results.BadRequest(new { Message = "Registro duplicado detectado. Essa preferência já existe." });
            }

            return Results.Created($"/seeting/{seeting.Id}", new
            {
                seeting.Id,
                seeting.UserId,
                seeting.CategoryId,
                seeting.AuthorId,
                seeting.SourceId,
                seeting.CreateDate
            });
        });

        route.MapGet("show", async (HttpContext http, AppDbContext context) =>
         {
             var userId = http.User.FindFirstValue("id");
             if (string.IsNullOrEmpty(userId)) return Results.Unauthorized();

             int usersId = int.Parse(userId);

             var seetings = await context.CustomizedResearches
                                             .Include(p => p.Category)
                                             .Include(p => p.Author)
                                             .Include(p => p.Source)
                                             .Where(p => p.UserId == usersId)
                                             .ToListAsync();

             if (!seetings.Any())
                 return Results.NotFound(new { Message = "Nenhuma preferência encontrada." });

             var categories = seetings
                 .Where(p => p.Category != null)
                 .Select(p => new { p.CategoryId, Nome = p.Category!.Name })
                 .Distinct()
                 .ToList();

             var authores = seetings
                 .Where(p => p.Author != null)
                 .Select(p => new { p.AuthorId, Nome = p.Author!.Name })
                 .Distinct()
                 .ToList();

             var source = seetings
                 .Where(p => p.Source != null)
                 .Select(p => new { p.SourceId, Nome = p.Source!.Name })
                 .Distinct()
                 .ToList();

             var result = new
             {
                 Categories = categories,
                 Authores = authores,
                 Fontes = source
             };

             return Results.Ok(result);
         });

        route.MapDelete("delete", async ([FromQuery] int? categoryId, [FromQuery] int? authorId, [FromQuery] int? sourceId, HttpContext http, AppDbContext context) =>
        {
            var userId = http.User.FindFirstValue("id");
            if (string.IsNullOrEmpty(userId)) return Results.Unauthorized();
            int usersId = int.Parse(userId);

            var seetings = await context.CustomizedResearches
                                         .Where(p => p.UserId == usersId &&
                                                ((categoryId.HasValue && p.CategoryId == categoryId) ||
                                                  (authorId.HasValue && p.AuthorId == authorId) ||
                                                  (sourceId.HasValue && p.SourceId == sourceId)))
                                         .ToListAsync();

            if (!seetings.Any())
                return Results.NotFound(new { Message = "Nenhuma preferência encontrada para os filtros informados." });

            context.CustomizedResearches.RemoveRange(seetings);
            await context.SaveChangesAsync();

            return Results.Ok(new { Message = "Preferência(s) deletada(s) com sucesso." });
        });
        
    }
}
