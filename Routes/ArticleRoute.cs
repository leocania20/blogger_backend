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
                    var userName = http.User.FindFirst("name")?.Value 
                                ?? http.User.Identity?.Name 
                                ?? "Usuário Desconhecido";
                    var userEmail = http.User.FindFirst("email")?.Value 
                                    ?? http.User.FindFirst(ClaimTypes.Email)?.Value 
                                    ?? http.User.FindFirst(ClaimTypes.Name)?.Value
                                    ?? "sememail@exemplo.com";


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

                                        AuthorModel? author = null;
                    var normalizedEmail = userEmail.Trim().ToLower();

                    var existingAuthor = await context.Authores
                        .FirstOrDefaultAsync(a => a.Email.ToLower() == normalizedEmail);

                    if (existingAuthor != null)
                    {
                        // Se já existir, reutiliza
                        author = existingAuthor;
                    }
                    else
                    {
                        // Caso contrário, cria um novo
                        author = new AuthorModel
                        {
                            Name = userName,
                            Email = normalizedEmail,
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
                        IsPublished = false,
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


            route.MapPut("/{id:int}/update", [Authorize] async (
                int id,
                HttpContext http,
                [FromForm] ArticleRequest req,
                AppDbContext context,
                IConfiguration config
            ) =>
            {
                try
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
                                sourceId = 1,
                                imagem = "nova_imagem.jpg"
                            }
                        });
                    var errors = ValidationHelper.ValidateModel(req);
                    if (errors.Any())
                        return Results.BadRequest(new
                        {
                            success = false,
                            message = "Os dados de atualização são inválidos.",
                            errors
                        });

                    var userIdClaim = http.User.FindFirst("id")?.Value;
                    if (string.IsNullOrEmpty(userIdClaim))
                        return Results.Unauthorized();

                    int userId = int.Parse(userIdClaim);
                    var author = await context.Authores.FirstOrDefaultAsync(a => a.UserId == userId);
                    if (author == null)
                    {
                        var user = await context.Users.FindAsync(userId);
                        if (user != null)
                        {
                            author = new AuthorModel
                            {
                                Name = user.Name,
                                Email = user.Email,
                                Bio = $"Autor criado automaticamente para o usuário {user.Name}",
                                UserId = user.Id
                            };
                            context.Authores.Add(author);
                            await context.SaveChangesAsync();
                        }
                        else
                        {
                            return Results.BadRequest(new { success = false, message = "Usuário associado não encontrado." });
                        }
                    }

                    var uploadsFolder = Path.Combine(Directory.GetCurrentDirectory(), "wwwroot", "uploads", "artigos");
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
                        if (!string.IsNullOrEmpty(article.Imagem))
                        {
                            try
                            {
                                var oldFile = Path.Combine(uploadsFolder, Path.GetFileName(article.Imagem));
                                if (File.Exists(oldFile)) File.Delete(oldFile);
                            }
                            catch { }
                        }
                    }

                    var baseUrl = config.GetValue<string>("BackendSettings:BaseUrl") ?? "http://localhost:5095";
                    var imageUrl = imageName != null
                        ? $"{baseUrl.TrimEnd('/')}/uploads/artigos/{imageName}"
                        : article.Imagem;
                    article.Title = req.Title;
                    article.Tag = req.Tag;
                    article.Text = req.Text;
                    article.Summary = req.Summary;
                    article.UpDate = DateTime.UtcNow;
                    article.IsPublished = req.IsPublished;
                    article.CategoryId = req.CategoryId;
                    article.SourceId = req.SourceId;
                    article.AuthorId = author.Id; 
                    article.Imagem = imageUrl;

                    await context.SaveChangesAsync();

                    return Results.Ok(new
                    {
                        success = true,
                        message = "Artigo atualizado com sucesso e autor definido automaticamente.",
                        data = new
                        {
                            article.Id,
                            article.Title,
                            article.Summary,
                            Author = author.Name,
                            article.Imagem
                        }
                    });
                }
                catch (DbUpdateException)
                {
                    return Results.Conflict(new
                    {
                        success = false,
                        message = "Erro de atualização: conflito nos dados do artigo."
                    });
                }
                catch (Exception ex)
                {
                    return ResponseHelper.ServerError($"Erro inesperado: {ex.Message}");
                }
            })
            .Accepts<ArticleRequest>("multipart/form-data")
            .WithSummary("Atualiza um Artigo existente com o Id")
            .WithDescription("Permite atualizar dados de um artigo e define o autor baseado no usuário autenticado.");


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
            }).WithSummary("Visualiza os dados completo dos artigo pelo ID(pagina inteira do artigo)")
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

            route.MapGet("/my-articles", [Authorize] async (
                HttpContext http,
                AppDbContext context,
                int? page) =>
            {
                try
                {
                    var userId = http.User.FindFirst("id")?.Value;
                    if (string.IsNullOrEmpty(userId))
                        return ResponseHelper.BadRequest("Usuário não autenticado.", null, new { Exemplo = new { Token = "Bearer seu_token_aqui" } });

                    int usuarioId = int.Parse(userId); 

                    var query = context.Articles
                        .Where(a => a.AuthorId == usuarioId && a.IsPublished == true)  
                        .Include(a => a.Author)
                        .Include(a => a.Category)
                        .Include(a => a.Source)
                        .OrderByDescending(a => a.CreateDate);

                    int currentPage = page ?? 1;
                    int pageSize = 6;
                    int total = await query.CountAsync();
                    int totalPages = (int)Math.Ceiling(total / (double)pageSize);

                    var articles = await query
                        .Skip((currentPage - 1) * pageSize)
                        .Take(pageSize)
                        .Select(a => new
                        {
                            a.Id,
                            a.Title,
                            a.Summary,
                            a.Text,
                            a.Imagem,
                            Category = a.Category != null ? a.Category.Name : "Sem categoria",
                            Author = a.Author != null ? a.Author.Name : "Autor desconhecido",
                            Source = a.Source != null ? a.Source.Name : "Sem fonte",
                            a.CreateDate
                        })
                        .ToListAsync();

                    if (articles == null || articles.Count == 0)
                    {
                        return ResponseHelper.Ok(new
                        {
                            page = currentPage,
                            pageSize,
                            total = 0,
                            totalPages = 0,
                            articles = new List<object>()
                        }, "Nenhum artigo encontrado para este usuário.");
                    }

                    return ResponseHelper.Ok(new
                    {
                        page = currentPage,
                        pageSize,
                        total,
                        totalPages,
                        articles
                    }, "Artigos criados pelo usuário logado retornados com sucesso.");
                }
                catch (Exception ex)
                {
                    return ResponseHelper.ServerError($"Erro ao buscar artigos do usuário: {ex.Message}");
                }
            })
            .WithSummary("Visualiza os artigos criados pelo usuário logado")
            .WithDescription("Retorna todos os artigos criados/postados pelo usuário autenticado, mostrando nomes de categoria, autor e fonte.");

            }
    }
}
