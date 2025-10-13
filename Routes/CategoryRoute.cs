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
                return Results.BadRequest(new { Errors = error });

            bool exist = await context.Categories.AnyAsync(c =>
                c.Active &&
                (c.Name.ToLower() == req.Name.ToLower() ||
                    c.Tag.ToLower() == req.Tag.ToLower()));

            if (exist)
                return Results.BadRequest(new { Message = "Já existe uma categoria com este nome ou tag." });

            var category = new CategoryModel
            {
                Name = req.Name,
                Description = req.Description,
                Tag = req.Tag,
                Active = req.Active
            };

            await context.Categories.AddAsync(category);
            await context.SaveChangesAsync();

            return Results.Created($"/category/{category.Id}", category);
        });

        route.MapPut("/{id:int}/update", async (int id, CategoryRequest req, AppDbContext context) =>
        {
            var category = await context.Categories.FirstOrDefaultAsync(c => c.Id == id && c.Active);
            if (category == null)
                return Results.NotFound(new { Message = "Categoria não encontrada." });

            var error = ValidationHelper.ValidateModel(req);
            if (error.Any())
                return Results.BadRequest(new { Errors = error });

            bool again = await context.Categories.AnyAsync(c =>
                c.Id != id &&
                c.Active &&
                (c.Name.ToLower() == req.Name.ToLower() ||
                    c.Tag.ToLower() == req.Tag.ToLower()));

            if (again)
                return Results.BadRequest(new { Message = "Já existe outra categoria com o mesmo nome ou tag." });

            category.Name = req.Name;
            category.Description = req.Description;
            category.Tag = req.Tag;
            category.Active = req.Active;

            await context.SaveChangesAsync();
            return Results.Ok(category);
        });
        
        route.MapGet("show", async (AppDbContext context) =>
        {
            var categories = await context.Categories
                                          .Where(c => c.Active)
                                          .ToListAsync();
            return Results.Ok(categories);
        });

        route.MapDelete("/{id:int}/delete", async (int id, AppDbContext context) =>
        {
            var category = await context.Categories.FirstOrDefaultAsync(c => c.Id == id && c.Active);
            if (category == null) return Results.NotFound();

            category.Active = false; 
            await context.SaveChangesAsync();
            return Results.Ok();
        });
    }
}
