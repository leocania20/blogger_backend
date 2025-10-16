using blogger_backend.Data;
using Microsoft.EntityFrameworkCore;
using Microsoft.AspNetCore.Authorization;

namespace blogger_backend.Routes
{
    public static class MonitoringRoute
    {
        public static void MonitoringRoutes(this WebApplication app)
        {
            var logs = app.MapGroup("/monitoring").WithTags("Monitoring").RequireAuthorization("Admin");

            logs.MapGet("/all", async (AppDbContext context) =>
            {
                var allLogs = await context.AccessLogs
                                           .OrderByDescending(l => l.AccessDate)
                                           .ToListAsync();

                if (!allLogs.Any())
                    return Results.NotFound(new { message = "Nenhum registro de acesso encontrado." });

                return Results.Ok(new
                {
                    message = "Todos os acessos recuperados com sucesso.",
                    total = allLogs.Count,
                    data = allLogs
                });
            }).WithSummary("Mostra todos os acessos registrados");

            logs.MapGet("/slow", async (AppDbContext context) =>
            {
                var slowRequests = await context.AccessLogs
                    .OrderByDescending(l => l.DurationMs)
                    .Take(50)
                    .ToListAsync();

                if (!slowRequests.Any())
                    return Results.NotFound(new { message = "Nenhuma rota lenta encontrada." });

                return Results.Ok(new
                {
                    message = "Rotas mais lentas recuperadas com sucesso.",
                    total = slowRequests.Count,
                    data = slowRequests
                });
            }).WithSummary("Mostra as 50 rotas mais lentas");

            logs.MapGet("/recent", async (AppDbContext context) =>
            {
                var recentLogs = await context.AccessLogs
                    .OrderByDescending(l => l.AccessDate)
                    .Take(50)
                    .ToListAsync();

                if (!recentLogs.Any())
                    return Results.NotFound(new { message = "Nenhum acesso recente encontrado." });

                return Results.Ok(new
                {
                    message = "Últimos acessos recuperados com sucesso.",
                    total = recentLogs.Count,
                    data = recentLogs
                });
            }).WithSummary("Mostra os últimos 50 acessos registrados");

            logs.MapGet("/most-accessed", async (AppDbContext context) =>
            {
                var mostAccessed = await context.AccessLogs
                    .GroupBy(l => l.Route)
                    .Select(g => new
                    {
                        Route = g.Key,
                        Count = g.Count(),
                        LastAccess = g.Max(x => x.AccessDate)
                    })
                    .OrderByDescending(g => g.Count)
                    .Take(10)
                    .ToListAsync();

                if (!mostAccessed.Any())
                    return Results.NotFound(new { message = "Nenhuma rota acessada encontrada." });

                return Results.Ok(new
                {
                    message = "Rotas mais acedidas recuperadas com sucesso.",
                    total = mostAccessed.Count,
                    data = mostAccessed
                });
            }).WithSummary("Mostra as 20 rotas mais acedidas");
        }
    }
}
