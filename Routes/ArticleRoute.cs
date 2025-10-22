using System.Security.Claims;
using blogger_backend.Data;
using blogger_backend.Models;
using blogger_backend.Utils;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;


namespace blogger_backend.Routes
{
    public static class ArticleRoute
    {
        public static void ArticlesRoutes(this WebApplication app)
        {
            var route = app.MapGroup("/articles").WithTags("Articles").RequireAuthorization();

            string SaveImage(IFormFile? imageFile, string uploadsFolder)
            {
                if (imageFile == null || imageFile.Length == 0)
                    return string.Empty;

                if (!Directory.Exists(uploadsFolder))
                    Directory.CreateDirectory(uploadsFolder);

                var uniqueFileName = Guid.NewGuid().ToString() + Path.GetExtension(imageFile.FileName);
                var filePath = Path.Combine(uploadsFolder, uniqueFileName);

                using (var stream = new FileStream(filePath, FileMode.Create))
                {
                    imageFile.CopyTo(stream);
                }

                return uniqueFileName;
            }

            route.MapPost("create", [Authorize, IgnoreAntiforgeryToken]
                async (HttpContext http, [FromForm] ArticleRequest req, AppDbContext context, IConfiguration config) =>
            {
                try
                {
                    var userIdClaim = http.User.FindFirst("id")?.Value;
                    var userName = http.User.FindFirst("name")?.Value ?? "Usuário Desconhecido";
                    var userEmail = http.User.FindFirst("email")?.Value ?? "sememail@exemplo.com";

                    if (string.IsNullOrEmpty(userIdClaim) || !int.TryParse(userIdClaim, out int userId))
                        return Results.Unauthorized();

                    var error = ValidationHelper.ValidateModel(req);
                    if (error.Any())
                        return Results.BadRequest(new { success = false, message = "Dados inválidos", error });
                    var uploadsFolder = Path.Combine(Directory.GetCurrentDirectory(), "wwwroot", "uploads", "artigos");
                    string imageName = SaveImage(req.Imagem, uploadsFolder);

                    var baseUrl = config.GetValue<string>("BackendSettings:BaseUrl") ?? "http://localhost:5095";
                    var imageUrl = !string.IsNullOrEmpty(imageName)
                        ? $"{baseUrl.TrimEnd('/')}/uploads/artigos/{imageName}"
                        : "";
                    var author = await context.Authores.FirstOrDefaultAsync(a => a.UserId == userId);
                    if (author == null)
                    {
                        author = new AuthorModel
                        {
                            Name = userName,
                            Email = userEmail,
                            Bio = "Autor criado automaticamente.",
                            UserId = userId
                        };
                        await context.Authores.AddAsync(author);
                        await context.SaveChangesAsync();
                    }

                    var article = new ArticlesModel
                    {
                        Title = req.Title,
                        Tag = req.Tag,
                        Text = req.Text,
                        Summary = req.Summary,
                        CreateDate = DateTime.UtcNow,
                        UpDate = DateTime.UtcNow,
                        IsPublished = req.IsPublished,
                        CategoryId = req.CategoryId,
                        SourceId = req.SourceId,
                        AuthorId = author.Id,
                        Imagem = imageUrl
                    };

                    await context.Articles.AddAsync(article);
                    await context.SaveChangesAsync();

                    return Results.Created($"/articles/{article.Id}", new
                    {
                        success = true,
                        message = "Artigo criado com sucesso.",
                        data = article
                    });
                }
                catch (Exception ex)
                {
                    return ResponseHelper.ServerError($"Erro inesperado: {ex.Message}");
                }
            })
                .Accepts<ArticleRequest>("multipart/form-data")
                .WithSummary("Cadastra Artigos ")
                .WithDescription("Permite enviar um artigo com imagem real e guardar em uploads/artigos.").DisableAntiforgery();


            route.MapPut("/{id:int}/update", [Authorize] async (int id, [FromForm] ArticleRequest req, AppDbContext context, IConfiguration config) =>
{
    var article = await context.Articles.FirstOrDefaultAsync(a => a.Id == id);
    if (article == null)
        return Results.NotFound(new
        {
            success = false,
            message = $"Artigo com ID {id} não encontrado.",
            example = new
            {
                id = 1,
                title = "Novo Título Atualizado",
                tag = "educação",
                text = "Conteúdo atualizado do artigo.",
                summary = "Resumo atualizado.",
                isPublished = true,
                categoryId = 1,
                authorId = 1,
                sourceId = 1,
                imagem = "nova_imagem.jpg"
            }
        });

    var error = ValidationHelper.ValidateModel(req);
    if (error.Any())
        return Results.BadRequest(new
        {
            success = false,
            message = "Os dados de atualização são inválidos.",
            error,
            example = new
            {
                title = "Artigo Atualizado",
                tag = "ciência",
                text = "Novo conteúdo válido.",
                summary = "Resumo breve atualizado.",
                isPublished = false,
                categoryId = 1,
                authorId = 1,
                sourceId = 1,
                imagem = "imagem_atualizada.jpg"
            }
        });

    // 📁 Caminho físico (Render usa /opt/render/project/src)
    var uploadsFolder = Path.Combine(Directory.GetCurrentDirectory(), "wwwroot", "uploads", "artigos");

    // 📸 Se uma nova imagem foi enviada, salva
    string? imageName = null;
    if (req.Imagem != null && req.Imagem.Length > 0)
    {
        if (!Directory.Exists(uploadsFolder))
            Directory.CreateDirectory(uploadsFolder);

        imageName = Guid.NewGuid().ToString() + Path.GetExtension(req.Imagem.FileName);
        var filePath = Path.Combine(uploadsFolder, imageName);

        using (var stream = new FileStream(filePath, FileMode.Create))
        {
            await req.Imagem.CopyToAsync(stream);
        }

        // Deleta a antiga se quiser (opcional)
        if (!string.IsNullOrEmpty(article.Imagem))
        {
            try
            {
                var oldFile = Path.Combine(uploadsFolder, Path.GetFileName(article.Imagem));
                if (File.Exists(oldFile)) File.Delete(oldFile);
            }
            catch { /* Ignorar erros ao deletar */ }
        }
    }

    // 🌐 Cria a URL pública se tiver imagem nova
    var baseUrl = config.GetValue<string>("BackendSettings:BaseUrl") ?? "http://localhost:5095";
    var imageUrl = imageName != null
        ? $"{baseUrl.TrimEnd('/')}/uploads/artigos/{imageName}"
        : article.Imagem; // mantém a antiga se não houver nova

    // 🔄 Atualiza os campos
    article.Title = req.Title;
    article.Tag = req.Tag;
    article.Text = req.Text;
    article.Summary = req.Summary;
    article.UpDate = DateTime.UtcNow;
    article.IsPublished = req.IsPublished;
    article.CategoryId = req.CategoryId;
    article.AuthorId = req.AuthorId;
    article.SourceId = req.SourceId;
    article.Imagem = imageUrl;

    try
    {
        await context.SaveChangesAsync();
        return Results.Ok(new
        {
            success = true,
            message = "Artigo atualizado com sucesso.",
            data = article
        });
    }
    catch (DbUpdateException)
    {
        return Results.Conflict(new
        {
            success = false,
            message = "Erro de atualização: conflito nos dados do artigo.",
        });
    }
    catch (Exception ex)
    {
        return ResponseHelper.ServerError($"Erro inesperado: {ex.Message}");
    }
})
                .Accepts<ArticleRequest>("multipart/form-data")
                .WithSummary("Atualiza um Artigo existente")
                .WithDescription("Permite atualizar dados")
                .Produces(StatusCodes.Status200OK)
                .Produces(StatusCodes.Status404NotFound)
                .Produces(StatusCodes.Status400BadRequest);


            route.MapDelete("/{id:int}/delete", async (int id, AppDbContext context) =>
            {
                var article = await context.Articles.FirstOrDefaultAsync(a => a.Id == id);
                if (article == null)
                    return Results.NotFound(new
                    {
                        success = false,
                        message = "Artigo não encontrado para exclusão.",
                        example = new { id = 1 }
                    });

                article.IsPublished = false;

                try
                {
                    await context.SaveChangesAsync();
                    return ResponseHelper.Ok("Artigo desativado com sucesso.");
                }
                catch (Exception ex)
                {
                    return ResponseHelper.ServerError($"Erro ao desativar artigo: {ex.Message}");
                }
            }).WithSummary("Deleta um Artigo pelo ID")
              .AllowAnonymous();

            route.MapGet("show-complet", async (int? id, AppDbContext context) =>
            {
                try
                {
                    var result = await HelpGetArticle.GetArticle(context, 1, 1, id, null, null, null, null, null, true);
                    return ResponseHelper.Ok(result);
                }
                catch (Exception ex)
                {
                    return ResponseHelper.ServerError($"Erro ao buscar artigo: {ex.Message}");
                }
            }).WithSummary("Visualiza os dados compleoto dos artigo pelo ID(pagina inteira do artigo)")
              .AllowAnonymous();

            route.MapGet("/show-short", async (int? page, AppDbContext context) =>
            {
                try
                {
                    var resultado = await HelpGetArticle.GetArticle(context, page ?? 1, 6);
                    return ResponseHelper.Ok(resultado);
                }
                catch (Exception ex)
                {
                    return ResponseHelper.ServerError($"Erro ao buscar artigos: {ex.Message}");
                }
            }).WithSummary("Visualizar artigos de forma resumida(página inicial)")
              .AllowAnonymous();

            route.MapGet("search", async (
                string? title,
                [FromQuery] int[]? categories,
                [FromQuery] int[]? sources,
                DateTime? date,
                int? page,
                AppDbContext context
            ) =>
            {
                try
                {
                    bool noParams = string.IsNullOrWhiteSpace(title)
                                    && (categories == null || categories.Length == 0)
                                    && (sources == null || sources.Length == 0)
                                    && !date.HasValue;

                    if (noParams)
                    {
                        return ResponseHelper.BadRequest(
                            "Deves informar pelo menos um parâmetro de pesquisa (title, category, source ou date).",
                            null,
                            new
                            {
                                Exemplo = new
                                {
                                    title = "tecnologia",
                                    categories = new[] { 1, 3 },
                                    sources = new[] { 2, 5 },
                                    date = "2025-10-14"
                                }
                            }
                        );
                    }

                    if (date.HasValue)
                    {
                        date = DateTime.SpecifyKind(date.Value, DateTimeKind.Utc);
                    }

                    var result = await HelpGetArticle.GetArticle(
                        context,
                        currentPage: page ?? 1,
                        pageSize: 6,
                        id: null,
                        title: string.IsNullOrWhiteSpace(title) ? null : title,
                        categoriesIds: categories != null && categories.Length > 0 ? categories.ToList() : null,
                        authorsIds: null,
                        sourcesIds: sources != null && sources.Length > 0 ? sources.ToList() : null,
                        date: date
                    );

                    return ResponseHelper.Ok(result, "Pesquisa realizada com sucesso.");
                }
                catch (Exception ex)
                {
                    return ResponseHelper.ServerError($"Erro ao buscar artigos: {ex.Message}");
                }
            })
                .WithSummary("Pesquisa e Visualiza Artigos por título, categorias, fontes ou data (sem usuário logado)")
                .AllowAnonymous();

            
            route.MapGet("/show-user", async (
                HttpContext http,
                AppDbContext context,
                int? page
            ) =>
            {
                try
                {
                    var userId = http.User.FindFirst("id")?.Value;
                    if (string.IsNullOrEmpty(userId))
                        return ResponseHelper.BadRequest("Usuário não autenticado.", null, new { Exemplo = new { Token = "Bearer seu_token_aqui" } });

                    int usersId = int.Parse(userId);

                    var preferences = await context.CustomizedResearches
                        .Where(p => p.UserId == usersId)
                        .ToListAsync();

                    if (preferences == null || preferences.Count == 0)
                    {
                        return ResponseHelper.Ok(new
                        {
                            page = 1,
                            pageSize = 6,
                            total = 0,
                            totalPages = 0,
                            articles = new List<object>()
                        }, "Nenhuma preferência encontrada para o usuário. Nenhum artigo disponível.");
                    }

                    var categoryIds = preferences.Where(p => p.CategoryId.HasValue).Select(p => p.CategoryId!.Value).Distinct().ToList();
                    var authorIds = preferences.Where(p => p.AuthorId.HasValue).Select(p => p.AuthorId!.Value).Distinct().ToList();
                    var sourceIds = preferences.Where(p => p.SourceId.HasValue).Select(p => p.SourceId!.Value).Distinct().ToList();

                    var result = await HelpGetArticle.GetArticle(
                        context,
                        currentPage: page ?? 1,
                        pageSize: 6,
                        id: null,
                        title: null,
                        categoriesIds: categoryIds,
                        authorsIds: authorIds,
                        sourcesIds: sourceIds,
                        date: null
                    );
                    dynamic dyn = result;
                    IEnumerable<object>? articles = null;

                    try
                    {
                        articles = (IEnumerable<object>?)dyn.articles;
                    }
                    catch
                    {
                        try { articles = (IEnumerable<object>?)dyn.Articles; } catch { articles = null; }
                    }

                    if (articles == null || !articles.Any())
                    {
                        return ResponseHelper.Ok(new
                        {
                            page = page ?? 1,
                            pageSize = 6,
                            total = 0,
                            totalPages = 0,
                            articles = new List<object>()
                        }, "Nenhum artigo encontrado com base nas preferências do usuário.");
                    }
                    return ResponseHelper.Ok(result, "Artigos recomendados com base nas preferências do usuário.");
                }
                catch (Exception ex)
                {
                    return ResponseHelper.ServerError($"Erro ao buscar artigos personalizados: {ex.Message}");
                }
            })
                .WithSummary("Visualiza os Artigos do Usuário logado")
                .WithDescription("Retorna artigos de acordo as configurações-preferências(categorias, autores e fontes).");

            route.MapGet("/suggest", async (string? query, AppDbContext context) =>
            {
                try
                {
                    if (string.IsNullOrWhiteSpace(query) || query.Length < 2)
                    {
                        return Results.BadRequest(new
                        {
                            success = false,
                            message = "Informe pelo menos 2 caracteres para fazer a sugestão.",
                            example = new { query = "tec" }
                        });
                    }

                    var suggestions = await context.Articles
                        .Where(a => a.IsPublished && a.Title.ToLower().Contains(query.ToLower()))
                        .OrderBy(a => a.Title)
                        .Select(a => a.Title)
                        .Distinct()
                        .Take(10)
                        .ToListAsync();

                    if (!suggestions.Any())
                    {
                        return ResponseHelper.Ok(new List<string>(), "Nenhuma sugestão encontrada.");
                    }

                    return ResponseHelper.Ok(new
                    {
                        total = suggestions.Count,
                        suggestions
                    }, "Sugestões obtidas com sucesso.");
                }
                catch (Exception ex)
                {
                    return ResponseHelper.ServerError($"Erro ao buscar sugestões: {ex.Message}");
                }
            })
                .WithSummary("Pesquisa automática de títulos de artigos")
                .WithDescription("Retorna até 10 sugestões de títulos que contêm o texto digitado.")
                .AllowAnonymous();
            }
    }
}
