using blogger_backend.Data;
using blogger_backend.Models;
using blogger_backend.Utils;    
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using System.Security.Claims;
namespace blogger_backend.Routes;

public static class ArticleRoute
{
    public static void ArticlesRoutes(this WebApplication app)
    {
        var route = app.MapGroup("/articles").WithTags("Articles").RequireAuthorization();

        string getImagem(string? nameImagem, IConfiguration config)
        {
            if (string.IsNullOrWhiteSpace(nameImagem))
                return "";

            var baseUrl = config.GetValue<string>("BackendSettings:BaseUrl") ?? "http://localhost:5095";
            return $"{baseUrl.TrimEnd('/')}/uploads/artigos/{nameImagem}";
        }

        route.MapPost("create", async (ArticleRequest req, AppDbContext context, IConfiguration config) =>
        {
            var errors = ValidationHelper.ValidateModel(req);
            if (errors.Any())
                return Results.BadRequest(new { Errors = errors });

            var imagemUrl = getImagem(req.Imagem, config);

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

            await context.Articles.AddAsync(article);
            await context.SaveChangesAsync();

            return Results.Created($"/article/{article.Id}", article);
        });

        route.MapPut("/{id:int}/update", async (int id, ArticleRequest req, AppDbContext context, IConfiguration config) =>
        {
            var article = await context.Articles.FirstOrDefaultAsync(a => a.Id == id);
            if (article == null)
                return Results.NotFound(new { Error = "Artigo não encontrado." });

            var errors = ValidationHelper.ValidateModel(req);
            if (errors.Any())
                return Results.BadRequest(new { Errors = errors });

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
                article.Imagem = getImagem(req.Imagem, config);

            await context.SaveChangesAsync();
            return Results.Ok(article);
        });

        route.MapDelete("/{id:int}/delete", async (int id, AppDbContext context) =>
        {
            var article = await context.Articles.FirstOrDefaultAsync(a => a.Id == id);
            if (article == null) return Results.NotFound();

            article.IsPublished = false;
            await context.SaveChangesAsync();
            return Results.Ok();
        });

        route.MapGet("show-complet", async (int? id, AppDbContext context) =>
        {
            int pageSize = 1;
            int currentPage = 1;

            var result = await HelpGetArticle.GetArticle(
                context, currentPage, pageSize, id, null, null, null, null, null, true);

            return Results.Ok(result);
        }).AllowAnonymous();

        route.MapGet("/show-short", async (int? page, AppDbContext context) =>
        {
            int pageSize = 6;
            int currentPage = page ?? 1;
            if (currentPage < 1) currentPage = 1;

            var resultado = await HelpGetArticle.GetArticle(context, currentPage, pageSize);
            return Results.Ok(resultado);
        }).AllowAnonymous();

        route.MapGet("show-preferencs", async (
            HttpContext http,
            AppDbContext context,
            [FromQuery] int? page) =>
        {
            var userId = http.User.FindFirstValue("id");
            if (string.IsNullOrEmpty(userId)) return Results.Unauthorized();

            int UserId = int.Parse(userId);
            int pageSize = 3;
            int currentPage = page ?? 1;
            if (currentPage < 1) currentPage = 1;

            var preferences = await context.CustomizedResearches
                .Where(c => c.UserId == UserId)
                .ToListAsync();

            if (!preferences.Any())
                return Results.NotFound(new { Message = "Nenhuma preferência encontrada para este usuário." });

            var categoriesIds = preferences.Where(p => p.CategoryId.HasValue).Select(p => p.CategoryId!.Value).ToList();
            var authorIds = preferences.Where(p => p.AuthorId.HasValue).Select(p => p.AuthorId!.Value).ToList();
            var sourceIds = preferences.Where(p => p.SourceId.HasValue).Select(p => p.SourceId!.Value).ToList();

            var result = await HelpGetArticle.GetArticle(
                context, currentPage, pageSize, null, null, categoriesIds, authorIds, sourceIds);

            return Results.Ok(result);
        });

        route.MapGet("/search", async (
            string? title,
            [FromQuery] int[]? categories,
            [FromQuery] int[]? sources,
            DateTime? date,
            int? page,
            AppDbContext context) =>
        {
            int pageSize = 6;
            int currentPage = page ?? 1;
            if (currentPage < 1) currentPage = 1;

            var categoriesIds = categories?.ToList() ?? new List<int>();
            var sourceIds = sources?.ToList() ?? new List<int>();

            var result = await HelpGetArticle.GetArticle(
                context,
                currentPage,
                pageSize,
                null,
                title,
                categoriesIds,
                null,
                sourceIds,
                date
            );

            return Results.Ok(result);
        }).AllowAnonymous();
    } 
}
