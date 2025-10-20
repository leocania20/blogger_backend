using Microsoft.EntityFrameworkCore;
using blogger_backend.Models;
using blogger_backend.Data;
using blogger_backend.Utils;
using Microsoft.AspNetCore.Mvc;
using System.Security.Claims;

public static class CustomizedResearchRoute
{
    public static void PesquisaCustomizadaRoutes(this WebApplication app)
    {
        var route = app.MapGroup("/settings").WithTags("Settings").RequireAuthorization();

        route.MapPost("/up", async (HttpContext http, List<CustomizedResearchRequest> reqList, AppDbContext context) =>
        {
            try
            {
                var userId = http.User.FindFirst("id")?.Value;
                if (string.IsNullOrEmpty(userId))
                    return ResponseHelper.BadRequest("Usuário não autenticado.", null, new
                    {
                        Exemplo = new { Token = "Bearer seu_token_aqui" }
                    });

                if (reqList == null || !reqList.Any())
                    return ResponseHelper.BadRequest("A lista de preferências não pode estar vazia.", null, new
                    {
                        Exemplo = new[]
                        {
                            new { CategoryId = 1, AuthorId = 0, SourceId = 0 },
                            new { CategoryId = 0, AuthorId = 2, SourceId = 0 }
                        }
                    });

                int usersId = int.Parse(userId);

                var savedList = new List<object>();
                var ignoredList = new List<object>();

                foreach (var req in reqList)
                {
                    bool hasCategory = req.CategoryId != 0;
                    bool hasAuthor = req.AuthorId != 0;
                    bool hasSource = req.SourceId != 0;

                    if (!hasCategory && !hasAuthor && !hasSource)
                        return ResponseHelper.BadRequest(
                            "Deve ser informado pelo menos um dos campos: CategoryId, AuthorId ou SourceId.",
                            null,
                            new { Exemplo = new { CategoryId = 1, AuthorId = 0, SourceId = 0 } }
                        );

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

                return ResponseHelper.Ok(new
                {
                    adicionados = savedList.Count,
                    ignorados = ignoredList.Count,
                    registros_adicionados = savedList,
                    registros_ignorados = ignoredList
                }, "Operação concluída com sucesso.");
            }
            catch (Exception ex)
            {
                return ResponseHelper.ServerError(ex.Message);
            }
        }).WithSummary("Cadastra ou Actualiza preferências personalizadas do usuário")
          .WithDescription("Permite adicionar categorias, autores e fontes específicas para personalizar a pesquisa.");

        route.MapGet("show", async (HttpContext http, AppDbContext context) =>
        {
            try
            {
                var userId = http.User.FindFirstValue("id");
                if (string.IsNullOrEmpty(userId))
                    return ResponseHelper.BadRequest("Usuário não autenticado.", null, new
                    {
                        Exemplo = new { Token = "Bearer seu_token_aqui" }
                    });

                int usersId = int.Parse(userId);

                var seetings = await context.CustomizedResearches
                                            .Include(p => p.Category)
                                            .Include(p => p.Author)
                                            .Include(p => p.Source)
                                            .Where(p => p.UserId == usersId)
                                            .ToListAsync();

                if (!seetings.Any())
                    return ResponseHelper.NotFound("Nenhuma preferência encontrada.");

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

                return ResponseHelper.Ok(result, "Preferências obtidas com sucesso.");
            }
            catch (Exception ex)
            {
                return ResponseHelper.ServerError(ex.Message);
            }
        }).WithSummary("Visualiza preferências personalizadas do Usuário Logado")
          .WithDescription("Visualiza as preferências personalizadas do usuário, incluindo categorias, autores e fontes.");
        
        route.MapDelete("delete", async ([FromQuery] int? categoryId, [FromQuery] int? authorId, [FromQuery] int? sourceId, HttpContext http, AppDbContext context) =>
        {
            try
            {
                var userId = http.User.FindFirstValue("id");
                if (string.IsNullOrEmpty(userId))
                    return ResponseHelper.BadRequest("Usuário não autenticado.", null, new
                    {
                        Exemplo = new { Token = "Bearer seu_token_aqui" }
                    });

                if (!categoryId.HasValue && !authorId.HasValue && !sourceId.HasValue)
                    return ResponseHelper.BadRequest(
                        "É necessário informar pelo menos um filtro para exclusão.",
                        null,
                        new { Exemplo = new { categoryId = 1, authorId = 0, sourceId = 0 } }
                    );

                int usersId = int.Parse(userId);

                var seetings = await context.CustomizedResearches
                                            .Where(p => p.UserId == usersId &&
                                                   ((categoryId.HasValue && p.CategoryId == categoryId) ||
                                                    (authorId.HasValue && p.AuthorId == authorId) ||
                                                    (sourceId.HasValue && p.SourceId == sourceId)))
                                            .ToListAsync();

                if (!seetings.Any())
                    return ResponseHelper.NotFound("Nenhuma preferência encontrada para os filtros informados.", new
                    {
                        Exemplo = new { categoryId = 1, authorId = 0, sourceId = 0 }
                    });

                context.CustomizedResearches.RemoveRange(seetings);
                await context.SaveChangesAsync();

                return ResponseHelper.Ok(new { Removidos = seetings.Count }, "Preferência(s) deletada(s) com sucesso.");
            }
            catch (Exception ex)
            {
                return ResponseHelper.ServerError(ex.Message);
            }
        }).WithSummary("Deleta preferências personalizadas do usuário logado");
    }
}
