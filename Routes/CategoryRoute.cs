using Microsoft.EntityFrameworkCore;
using blogger_backend.Models;
using blogger_backend.Data;
using blogger_backend.Utils;

namespace blogger_backend.Routes;

public static class CategoryRoute
{
    public static void CategoryRoutes(this WebApplication app)
    {
        var route = app.MapGroup("/category").WithTags("Categories");

        route.MapPost("create", async (CategoryRequest req, AppDbContext context) =>
        {
            var error = ValidationHelper.ValidateModel(req);
            if (error.Any())
                return ResponseHelper.BadRequest(
                    "Os dados enviados são inválidos.",
                    error,
                    new
                    {
                        name = "Tecnologia",
                        description = "Categoria relacionada a artigos sobre tecnologia.",
                        tag = "tech",
                        active = true
                    }
                );

            bool exist = await context.Categories.AnyAsync(c =>
                c.Active &&
                (c.Name.ToLower() == req.Name.ToLower() ||
                 c.Tag.ToLower() == req.Tag.ToLower()));

            if (exist)
                return ResponseHelper.Conflict(
                    "Já existe uma categoria com este nome ou tag.",
                    new
                    {
                        name = "Inovação",
                        description = "Artigos sobre novas tendências.",
                        tag = "innovation",
                        active = true
                    }
                );

            var category = new CategoryModel
            {
                Name = req.Name.Trim(),
                Description = req.Description?.Trim(),
                Tag = req.Tag.Trim().ToLower(),
                Active = req.Active
            };

            try
            {
                await context.Categories.AddAsync(category);
                await context.SaveChangesAsync();

                return ResponseHelper.Created($"/category/{category.Id}", new
                {
                    category.Id,
                    category.Name,
                    category.Description,
                    category.Tag,
                    category.Active
                }, "Categoria criada com sucesso.");
            }
            catch (Exception ex)
            {
                return ResponseHelper.ServerError($"Erro ao criar categoria: {ex.Message}");
            }
        }).WithSummary("Cadastra Categorias");

        route.MapPut("/{id:int}/update", async (int id, CategoryRequest req, AppDbContext context) =>
        {
            var category = await context.Categories.FirstOrDefaultAsync(c => c.Id == id && c.Active);
            if (category == null)
                return ResponseHelper.NotFound(
                    $"Categoria com ID {id} não encontrada.",
                    new
                    {
                        id = 1,
                        name = "Educação",
                        description = "Conteúdos sobre ensino e formação.",
                        tag = "education",
                        active = true
                    }
                );

            var errors = ValidationHelper.ValidateModel(req);
            if (errors.Any())
                return ResponseHelper.BadRequest(
                    "Os dados enviados são inválidos.",
                    errors,
                    new
                    {
                        name = "Negócios",
                        description = "Artigos sobre gestão e finanças.",
                        tag = "business",
                        active = true
                    }
                );

            bool duplicate = await context.Categories.AnyAsync(c =>
                c.Id != id &&
                c.Active &&
                (c.Name.ToLower() == req.Name.ToLower() ||
                 c.Tag.ToLower() == req.Tag.ToLower()));

            if (duplicate)
                return ResponseHelper.Conflict(
                    "Já existe outra categoria com o mesmo nome ou tag.",
                    new
                    {
                        name = "Saúde",
                        description = "Categoria de artigos sobre bem-estar e medicina.",
                        tag = "health",
                        active = true
                    }
                );

            category.Name = req.Name.Trim();
            category.Description = req.Description?.Trim();
            category.Tag = req.Tag.Trim().ToLower();
            category.Active = req.Active;

            try
            {
                await context.SaveChangesAsync();

                return ResponseHelper.Ok(new
                {
                    category.Id,
                    category.Name,
                    category.Description,
                    category.Tag,
                    category.Active
                }, "Categoria atualizada com sucesso.");
            }
            catch (Exception ex)
            {
                return ResponseHelper.ServerError($"Erro ao atualizar categoria: {ex.Message}");
            }
        }).WithSummary("Atualiza uma Categoria pelo ID");

        route.MapGet("show", async (AppDbContext context) =>
        {
            var categories = await context.Categories
                .Where(c => c.Active)
                .Select(c => new
                {
                    c.Id,
                    c.Name
                })
                .ToListAsync();

            if (!categories.Any())
            {
                return ResponseHelper.NotFound(
                    "Nenhuma categoria ativa encontrada.",
                    new
                    {
                        exemplo = "Crie uma nova categoria via POST /category/create"
                    }
                );
            }

            return ResponseHelper.Ok(categories, "Lista de categorias ativas.");
        }).WithSummary("Visualiza Categorias Ativas");

        route.MapDelete("/{id:int}/delete", async (int id, AppDbContext context) =>
        {
            var category = await context.Categories.FirstOrDefaultAsync(c => c.Id == id && c.Active);
            if (category == null)
                return ResponseHelper.NotFound(
                    $"Categoria com ID {id} não encontrada.",
                    new { id = 1 }
                );

            category.Active = false;

            try
            {
                await context.SaveChangesAsync();
                return ResponseHelper.Ok(new
                {
                    category.Id,
                    category.Name,
                    category.Tag
                }, "Categoria desativada com sucesso.");
            }
            catch (Exception ex)
            {
                return ResponseHelper.ServerError($"Erro ao desativar categoria: {ex.Message}");
            }
        }).WithSummary("Deleta uma Categoria pelo ID");
    }
}
