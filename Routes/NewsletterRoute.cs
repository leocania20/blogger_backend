using System.Security.Claims;
using blogger_backend.Data;
using blogger_backend.Models;
using blogger_backend.Utils;
using Microsoft.AspNetCore.Authorization;
using Microsoft.EntityFrameworkCore;

namespace blogger_backend.Routes
{
    public static class NewsletterRoutes
    {
        public static void NewsletterRoute(this WebApplication app)
        {
            var route = app.MapGroup("/newsletter").WithTags("Newsletter");

            route.MapPost("/subscribe", async ( HttpContext http, AppDbContext db, NewsletterModel req) =>
            {
                try
                {
                    string? email =
                        http.User.FindFirst(ClaimTypes.Email)?.Value ??
                        http.User.FindFirst("email")?.Value ??
                        http.User.FindFirst("Email")?.Value ??
                        http.User.FindFirst("unique_name")?.Value ??
                        req.Email;

                    if (string.IsNullOrEmpty(email))
                        return ResponseHelper.BadRequest("E-mail é obrigatório.", null, new { exemplo = new { email = "exemplo@dominio.com" } });

                    bool exists = await db.Newsletters.AnyAsync(n => n.Email == email);
                    if (exists)
                        return ResponseHelper.Ok("Já inscrito na newsletter.");

                    var model = new NewsletterModel
                    {
                        Email = email,
                        Active = true,
                        UnsubscribeToken = Guid.NewGuid().ToString()
                    };

                    db.Newsletters.Add(model);
                    await db.SaveChangesAsync();

                    return ResponseHelper.Ok("Inscrição efetuada com sucesso.");
                }
                catch (Exception ex)
                {
                    return ResponseHelper.ServerError($"Erro ao subscrever: {ex.Message}");
                }
            })
                .WithSummary("Inscrever na newsletter")
                .WithDescription("Permite que um usuário, autenticado ou não, se inscreva na newsletter fornecendo seu e-mail.");      
                
            route.MapGet("/unsubscribe/{token}", async (string token, AppDbContext db) =>
            {
                try
                {
                    var subs = await db.Newsletters.FirstOrDefaultAsync(n => n.UnsubscribeToken == token);
                    if (subs == null)
                        return Results.NotFound(new { success = false, message = "Token inválido." });

                    subs.Active = false;
                    await db.SaveChangesAsync();

                    return ResponseHelper.Ok("Inscrição cancelada com sucesso.");
                }
                catch (Exception ex)
                {
                    return ResponseHelper.ServerError($"Erro ao cancelar inscrição: {ex.Message}");
                }
            })
                .WithSummary("Cancelar inscrição")
                .WithDescription("Permite o cancelamento via link público enviado no e-mail.")
                .AllowAnonymous();

            route.MapGet("/list", [Authorize(Policy = "admin")] async (AppDbContext db) =>
            {
                var list = await db.Newsletters
                    .OrderByDescending(n => n.CreateDate)
                    .Select(n => new { n.Id, n.Email, n.Active, n.CreateDate })
                    .ToListAsync();

                return ResponseHelper.Ok(list);
            })
            .WithSummary("Lista todos os inscritos")
            .WithDescription("Somente administradores podem ver a lista de inscritos na newsletter.");


            route.MapPost("/send", [Authorize(Policy = "admin")] async (
                NewsletterSendRequest req,
                AppDbContext db,
                SendGridEmailServices emailService,
                IConfiguration config) =>
            {
                try
                {
                    var query = db.Newsletters.AsQueryable();
                    if (req.OnlyActive)
                        query = query.Where(n => n.Active);

                    var list = await query.ToListAsync();
                    if (!list.Any())
                        return ResponseHelper.BadRequest("Nenhum inscrito ativo.");

                    int sentCount = 0;
                    int failCount = 0;

                    int batchSize = int.Parse(config["Newsletter:BatchSize"] ?? "50"); 
                    int delayMsBetweenEmails = int.Parse(config["Newsletter:DelayMs"] ?? "200"); 
                    int maxRetries = int.Parse(config["Newsletter:MaxRetries"] ?? "2"); 

                    string baseUrl = config["BackendSettings:BaseUrl"] ?? "http://localhost:5095";

                    for (int i = 0; i < list.Count; i += batchSize)
                    {
                        var batch = list.Skip(i).Take(batchSize).ToList();

                        foreach (var subscriber in batch)
                        {
                            string to = subscriber.Email;

                            string html = $"{req.HtmlBody}<br><br><a href='{baseUrl}/newsletter/unsubscribe/{subscriber.UnsubscribeToken}'>Cancelar inscrição</a>";

                            bool ok = false;
                            string responseBody = "";
                            int? statusCode = null;

                            for (int attempt = 0; attempt <= maxRetries && !ok; attempt++)
                            {
                                try
                                {
                                    var response = await emailService.SendEmailWithResponseAsync(to, req.Subject, html, req.PlainText);
                                    statusCode = (int)response.StatusCode;
                                    responseBody = await response.Body.ReadAsStringAsync();
                                    ok = response.IsSuccessStatusCode;
                                }
                                catch (Exception ex)
                                {
                                    responseBody = ex.Message;
                                    ok = false;
                                }

                                if (!ok && attempt < maxRetries)
                                    await Task.Delay(500 * (attempt + 1)); 
                            }

                            var log = new NewsletterLog
                            {
                                NewsletterId = subscriber.Id,
                                ToEmail = to,
                                Subject = req.Subject,
                                Success = ok,
                                ResponseBody = responseBody,
                                StatusCode = statusCode
                            };
                            db.NewsletterLogs.Add(log);
                            await db.SaveChangesAsync(); 
                            if (ok) sentCount++; else failCount++;

                            await Task.Delay(delayMsBetweenEmails);
                        }
                        await Task.Delay(1000);
                    }

                    return ResponseHelper.Ok(new { enviados = sentCount, falhas = failCount }, "Emails enviado para todos os subescritos.");
                }
                catch (Exception ex)
                {
                    return ResponseHelper.ServerError($"Erro ao enviar newsletter: {ex.Message}");
                }
            })
            .WithSummary("Envia newsletter a todos os inscritos")
            .WithDescription("Envia em batches, grava logs e realiza retries. Use com cautela para grandes listas.");

          
        }
    }
}
