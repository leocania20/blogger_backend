using Microsoft.EntityFrameworkCore;
using blogger_backend.Models;
using blogger_backend.Data;
using blogger_backend.Utils;

namespace blogger_backend.Routes;

public static class AutorRoute
{
    public static void AuthorRoutes(this WebApplication app)
    {
        var route = app.MapGroup("/author").WithTags("Authors");

        route.MapPost("create", async (AuthorRequest req, AppDbContext context) =>
        {
            var error = ValidationHelper.ValidateModel(req);
            if (error.Any())
                return ResponseHelper.BadRequest(
                    "Os dados enviados são inválidos.",
                    error,
                    new
                    {
                        name = "José Silva",
                        bio = "Autor especializado em tecnologia e inovação.",
                        email = "jose.silva@example.com",
                        userId = 1
                    }
                );

            bool exists = await context.Authores.AnyAsync(a =>
                a.Active &&
                (a.Email.ToLower() == req.Email.ToLower() ||
                 a.Name.ToLower() == req.Name.ToLower()));

            if (exists)
                return ResponseHelper.Conflict(
                    "Já existe um autor com este nome ou e-mail.",
                    new
                    {
                        name = "Novo Autor",
                        bio = "Breve descrição sobre o autor.",
                        email = "novo.email@example.com",
                        userId = 1
                    }
                );

            var author = new AuthorModel
            {
                Name = req.Name.Trim(),
                Bio = req.Bio.Trim(),
                Email = req.Email.Trim().ToLower(),
                UserId = req.UserId,
                Active = true
            };

            try
            {
                await context.Authores.AddAsync(author);
                await context.SaveChangesAsync();

                return ResponseHelper.Created($"/author/{author.Id}", new
                {
                    author.Id,
                    author.Name,
                    author.Email,
                    author.Bio,
                    author.UserId,
                    author.Active
                }, "Autor criado com sucesso.");
            }
            catch (Exception ex)
            {
                return ResponseHelper.ServerError($"Erro ao criar autor: {ex.Message}");
            }
        }).WithSummary("Cadastra Autores"). Produces(400);

        route.MapPut("/{id:int}/update", async (int id, AuthorRequest req, AppDbContext context) =>
        {
            var author = await context.Authores.FirstOrDefaultAsync(a => a.Id == id && a.Active);
            if (author == null)
                return ResponseHelper.NotFound(
                    $"Autor com ID {id} não encontrado.",
                    new
                    {
                        id = 1,
                        name = "José Silva",
                        bio = "Atualização da biografia.",
                        email = "jose.silva@update.com",
                        userId = 1
                    }
                );

            var error = ValidationHelper.ValidateModel(req);
            if (error.Any())
                return ResponseHelper.BadRequest(
                    "Os dados enviados são inválidos.",
                    error,
                    new
                    {
                        name = "João Pereira",
                        bio = "Autor experiente em ciência de dados.",
                        email = "joao.pereira@example.com",
                        userId = 2
                    }
                );

            bool duplicate = await context.Authores.AnyAsync(a =>
                a.Id != id &&
                a.Active &&
                (a.Email.ToLower() == req.Email.ToLower() ||
                 a.Name.ToLower() == req.Name.ToLower()));

            if (duplicate)
                return ResponseHelper.Conflict(
                    "Outro autor com o mesmo nome ou e-mail já existe.",
                    new
                    {
                        name = "Maria Costa",
                        bio = "Autora de artigos sobre economia.",
                        email = "maria.costa@example.com",
                        userId = 3
                    }
                );

            author.Name = req.Name.Trim();
            author.Bio = req.Bio.Trim();
            author.Email = req.Email.Trim().ToLower();
            author.UserId = req.UserId;

            try
            {
                await context.SaveChangesAsync();

                return ResponseHelper.Ok(new
                {
                    author.Id,
                    author.Name,
                    author.Email,
                    author.Bio,
                    author.UserId,
                    author.Active
                }, "Autor atualizado com sucesso.");
            }
            catch (Exception ex)
            {
                return ResponseHelper.ServerError($"Erro ao atualizar autor: {ex.Message}");
            }
        }).WithSummary("Atualiza um Autor existente pelo ID");

        route.MapGet("show", async (AppDbContext context) =>
        {
            var autores = await context.Authores
                .Where(a => a.Active)
                .Select(a => new
                {
                    a.Id,
                    a.Name
                })
                .ToListAsync();

            if (!autores.Any())
            {
                return ResponseHelper.NotFound(
                    "Nenhum autor ativo encontrado.",
                    new
                    {
                        exemplo = "Crie um autor usando POST /author/create"
                    }
                );
            }

            return ResponseHelper.Ok(autores, "Lista de autores ativos.");
        }).WithSummary("Visualiza todos os Autores ativos");

        route.MapDelete("/{id:int}/delete", async (int id, AppDbContext context) =>
        {
            var author = await context.Authores.FirstOrDefaultAsync(a => a.Id == id && a.Active);
            if (author == null)
                return ResponseHelper.NotFound(
                    $"Autor com ID {id} não encontrado.",
                    new
                    {
                        id = 1
                    }
                );

            author.Active = false;

            try
            {
                await context.SaveChangesAsync();
                return ResponseHelper.Ok(new
                {
                    author.Id,
                    author.Name,
                    author.Email
                }, "Autor desativado com sucesso.");
            }
            catch (Exception ex)
            {
                return ResponseHelper.ServerError($"Erro ao desativar autor: {ex.Message}");
            }
        }).WithSummary("Deeleta um Autor pelo ID");

        

    }
}
