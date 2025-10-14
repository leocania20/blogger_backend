using blogger_backend.Data;
using blogger_backend.Models;
using blogger_backend.Utils;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using System.Security.Claims;

namespace blogger_backend.Routes
{
    public static class ArticleRoute
    {
        public static void ArticlesRoutes(this WebApplication app)
        {
            var route = app.MapGroup("/articles").WithTags("Articles").RequireAuthorization();

            string GetImagem(string? nameImagem, IConfiguration config)
            {
                if (string.IsNullOrWhiteSpace(nameImagem))
                    return "";
                var baseUrl = config.GetValue<string>("BackendSettings:BaseUrl") ?? "http://localhost:5095";
                return $"{baseUrl.TrimEnd('/')}/uploads/artigos/{nameImagem}";
            }

            route.MapPost("create", async (ArticleRequest req, AppDbContext context, IConfiguration config) =>
            {
                var error = ValidationHelper.ValidateModel(req);
                if (error.Any())
                    return Results.BadRequest(new
                    {
                        success = false,
                        message = "Os dados enviados são inválidos.",
                        error,
                        example = new
                        {
                            title = "Exemplo de Título",
                            tag = "educação",
                            text = "Texto completo do artigo.",
                            summary = "Resumo breve sobre o artigo.",
                            isPublished = true,
                            categoryId = 1,
                            authorId = 1,
                            sourceId = 1,
                            imagem = "exemplo.jpg"
                        }
                    });

                var imagemUrl = GetImagem(req.Imagem, config);

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
                    AuthorId = req.AuthorId,
                    SourceId = req.SourceId,
                    Imagem = imagemUrl
                };

                try
                {
                    await context.Articles.AddAsync(article);
                    await context.SaveChangesAsync();
                    return ResponseHelper.Created($"/articles/{article.Id}", article);
                }
                catch (DbUpdateException)
                {
                    return Results.Conflict(new
                    {
                        success = false,
                        message = "Conflito: o artigo pode já existir. Verifique o título ou identificadores.",
                        example = new
                        {
                            title = "Um novo artigo exclusivo",
                            tag = "animais",
                            text = "Conteúdo original e sem duplicação.",
                            summary = "Resumo diferente do existente.",
                            isPublished = true,
                            categoryId = 2,
                            authorId = 1,
                            sourceId = 1,
                            imagem = "artigo_novo.jpg"
                        }
                    });
                }
                catch (Exception ex)
                {
                    return ResponseHelper.ServerError($"Erro inesperado: {ex.Message}");
                }
            }).AllowAnonymous();

            route.MapPut("/{id:int}/update", async (int id, ArticleRequest req, AppDbContext context, IConfiguration config) =>
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

                article.Title = req.Title;
                article.Tag = req.Tag;
                article.Text = req.Text;
                article.Summary = req.Summary;
                article.UpDate = DateTime.UtcNow;
                article.IsPublished = req.IsPublished;
                article.CategoryId = req.CategoryId;
                article.AuthorId = req.AuthorId;
                article.SourceId = req.SourceId;

                if (!string.IsNullOrWhiteSpace(req.Imagem))
                    article.Imagem = GetImagem(req.Imagem, config);

                try
                {
                    await context.SaveChangesAsync();
                    return ResponseHelper.Ok(article);
                }
                catch (DbUpdateException)
                {
                    return Results.Conflict(new
                    {
                        success = false,
                        message = "Erro de atualização: conflito nos dados do artigo.",
                        example = new
                        {
                            title = "Título diferente de outro artigo existente",
                            tag = "história",
                            text = "Texto alterado sem duplicação.",
                            summary = "Novo resumo original.",
                            isPublished = true,
                            categoryId = 2,
                            authorId = 1,
                            sourceId = 2,
                            imagem = "imagem_valida.jpg"
                        }
                    });
                }
                catch (Exception ex)
                {
                    return ResponseHelper.ServerError($"Erro inesperado: {ex.Message}");
                }
            });

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
            });

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
            }).AllowAnonymous();

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
            }).AllowAnonymous();

            route.MapGet("search", async (
                string? title,
                [FromQuery] int[]? categories,
                [FromQuery] int[]? sources,
                DateTime? date,
                int? page,
                AppDbContext context) =>
            {
                try
                {
                    var result = await HelpGetArticle.GetArticle(
                        context,
                        page ?? 1,
                        6,
                        null,
                        title,
                        categories?.ToList() ?? new(),
                        null,
                        sources?.ToList() ?? new(),
                        date
                    );
                    return ResponseHelper.Ok(result);
                }
                catch (Exception ex)
                {
                    return ResponseHelper.ServerError($"Erro ao buscar artigos: {ex.Message}");
                }
            }).AllowAnonymous();
        }
    }
}
