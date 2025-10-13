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

        route.MapPost("/settings/up", async (HttpContext http, List<CustomizedResearchRequest> reqList, AppDbContext context) =>
        {
            var userId = http.User.FindFirst("id")?.Value;
            if (string.IsNullOrEmpty(userId))
                return Results.Unauthorized();

            int usersId = int.Parse(userId);

            var savedList = new List<object>();
            var ignoredList = new List<object>();

            foreach (var req in reqList)
            {
                bool hasCategory = req.CategoryId != 0;
                bool hasAuthor = req.AuthorId != 0;
                bool hasSource = req.SourceId != 0;

                if (!hasCategory && !hasAuthor && !hasSource)
                {
                    return Results.BadRequest("Deve ser informado pelo menos um dos campos: CategoryId, AuthorId ou SourceId.");
                }
                bool alreadyExists = false;
                if (hasCategory)
                {
                    alreadyExists = await context.CustomizedResearches
                        .AnyAsync(c => c.UserId == usersId && c.CategoryId == req.CategoryId);

                    if (alreadyExists)
                    {
                        ignoredList.Add(new { Tipo = "Categoria", Id = req.CategoryId });
                        continue; 
                    }
                }

                if (hasAuthor)
                {
                    alreadyExists = await context.CustomizedResearches
                        .AnyAsync(c => c.UserId == usersId && c.AuthorId == req.AuthorId);

                    if (alreadyExists)
                    {
                        ignoredList.Add(new { Tipo = "Autor", Id = req.AuthorId });
                        continue;
                    }
                }

                if (hasSource)
                {
                    alreadyExists = await context.CustomizedResearches
                        .AnyAsync(c => c.UserId == usersId && c.SourceId == req.SourceId);

                    if (alreadyExists)
                    {
                        ignoredList.Add(new { Tipo = "Fonte", Id = req.SourceId });
                        continue;
                    }
                }
                var newResearch = new CustomizedResearchModel
                {
                    UserId = usersId,
                    CategoryId = hasCategory ? req.CategoryId : null,
                    AuthorId = hasAuthor ? req.AuthorId : null,
                    SourceId = hasSource ? req.SourceId : null,
                    CreateDate = DateTime.UtcNow
                };

                await context.CustomizedResearches.AddAsync(newResearch);
                await context.SaveChangesAsync();

                savedList.Add(new
                {
                    newResearch.Id,
                    newResearch.UserId,
                    newResearch.CategoryId,
                    newResearch.AuthorId,
                    newResearch.SourceId,
                    newResearch.CreateDate
                });
            }
            return Results.Ok(new
            {
                mensagem = "Operação concluída com sucesso.",
                adicionados = savedList.Count,
                ignorados = ignoredList.Count,
                registros_adicionados = savedList,
                registros_ignorados = ignoredList
            });

        })
        .WithName("CustomizedResearchCreate")
        .WithSummary("Cadastra pesquisas personalizadas (Categoria, Autor ou Fonte)")
        .WithDescription("Permite cadastrar uma ou várias pesquisas personalizadas associadas a um usuário autenticado. Ignora itens duplicados já cadastrados para o mesmo usuário.")
        .Produces(StatusCodes.Status200OK)
        .Produces(StatusCodes.Status400BadRequest)
        .Produces(StatusCodes.Status401Unauthorized);

    
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
