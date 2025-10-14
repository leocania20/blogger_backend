using Microsoft.EntityFrameworkCore;
using blogger_backend.Models;
using blogger_backend.Data;
using blogger_backend.Utils;
using blogger_backend.Requests;

namespace blogger_backend.Routes;

public static class FonteRoute
{
    public static void SourceRoute(this WebApplication app)
    {
        var route = app.MapGroup("/source").WithTags("Sources");

        route.MapPost("create", async (SourceRequest req, AppDbContext context) =>
        {
            try
            {
                var error = ValidationHelper.ValidateModel(req);
                if (error.Any())
                    return ResponseHelper.BadRequest(
                        "Os dados enviados são inválidos.",
                        error,
                        new
                        {
                            Name = "BBC News",
                            URL = "https://www.bbc.com",
                            Type = "Notícias"
                        });

                bool exist = await context.Sources.AnyAsync(f =>
                    f.Active &&
                    (f.Name.ToLower() == req.Name.ToLower() ||
                     f.URL.ToLower() == req.URL.ToLower()));

                if (exist)
                    return ResponseHelper.Conflict(
                        "Já existe uma fonte com este nome ou URL.",
                        new
                        {
                            Name = "CNN Portugal",
                            URL = "https://cnnportugal.iol.pt",
                            Type = "Jornal"
                        });

                var source = new SourceModel
                {
                    Name = req.Name,
                    URL = req.URL,
                    Type = req.Type,
                    Active = true
                };

                await context.Sources.AddAsync(source);
                await context.SaveChangesAsync();

                return ResponseHelper.Created(
                    $"/source/{source.Id}",
                    new
                    {
                        source.Id,
                        source.Name,
                        source.URL,
                        source.Type,
                        source.Active
                    },
                    "Fonte criada com sucesso.");
            }
            catch (Exception ex)
            {
                return ResponseHelper.ServerError($"Erro ao criar fonte: {ex.Message}");
            }
        });

        route.MapPut("/{id:int}/update", async (int id, SourceRequest req, AppDbContext context) =>
        {
            try
            {
                var error = ValidationHelper.ValidateModel(req);
                if (error.Any())
                    return ResponseHelper.BadRequest(
                        "Os dados enviados são inválidos.",
                        error,
                        new
                        {
                            Name = "Reuters",
                            URL = "https://www.reuters.com",
                            Type = "Agência"
                        });

                var source = await context.Sources.FirstOrDefaultAsync(f => f.Id == id && f.Active);
                if (source == null)
                    return ResponseHelper.NotFound(
                        "Fonte não encontrada.",
                        new { Id = id });

                bool again = await context.Sources.AnyAsync(f =>
                    f.Id != id &&
                    f.Active &&
                    (f.Name.ToLower() == req.Name.ToLower() ||
                     f.URL.ToLower() == req.URL.ToLower()));

                if (again)
                    return ResponseHelper.Conflict(
                        "Já existe outra fonte com o mesmo nome ou URL.",
                        new
                        {
                            Name = "Agência Angola Press",
                            URL = "https://www.angop.ao",
                            Type = "Agência"
                        });

                source.Name = req.Name;
                source.URL = req.URL;
                source.Type = req.Type;

                await context.SaveChangesAsync();

                return ResponseHelper.Ok(
                    new
                    {
                        source.Id,
                        source.Name,
                        source.URL,
                        source.Type,
                        source.Active
                    },
                    "Fonte atualizada com sucesso.");
            }
            catch (Exception ex)
            {
                return ResponseHelper.ServerError($"Erro ao atualizar fonte: {ex.Message}");
            }
        });

        route.MapGet("show", async (AppDbContext context) =>
        {
            try
            {
                var sources = await context.Sources
                                          .Where(f => f.Active)
                                          .Select(f => new
                                          {
                                              f.Id,
                                              f.Name
                                          })
                                          .ToListAsync();

                if (!sources.Any())
                    return ResponseHelper.NotFound("Nenhuma fonte encontrada.", new { Exemplo = "Adicione fontes através do endpoint /source/create" });

                return ResponseHelper.Ok(
                    sources,
                    "Lista de fontes ativas obtida com sucesso.");
            }
            catch (Exception ex)
            {
                return ResponseHelper.ServerError($"Erro ao obter lista de fontes: {ex.Message}");
            }
        });

        route.MapDelete("/{id:int}/delete", async (int id, AppDbContext context) =>
        {
            try
            {
                var source = await context.Sources.FirstOrDefaultAsync(f => f.Id == id && f.Active);
                if (source == null)
                    return ResponseHelper.NotFound(
                        "Fonte não encontrada.",
                        new { Id = id });

                source.Active = false;
                await context.SaveChangesAsync();

                return ResponseHelper.Ok(
                    new
                    {
                        source.Id,
                        source.Name,
                        source.URL
                    },
                    "Fonte desativada com sucesso.");
            }
            catch (Exception ex)
            {
                return ResponseHelper.ServerError($"Erro ao desativar fonte: {ex.Message}");
            }
        });
    }
}
