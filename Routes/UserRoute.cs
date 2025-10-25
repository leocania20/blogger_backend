using Microsoft.EntityFrameworkCore;
using blogger_backend.Models;
using blogger_backend.Data;
using blogger_backend.Utils;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Authorization;
using System.Security.Claims;
using System.IdentityModel.Tokens.Jwt;

namespace blogger_backend.Routes;

public static class UserRoute
{
    public static void UserRoutes(this WebApplication app, IConfiguration config)
    {
        var route = app.MapGroup("/user").WithTags("Users");

        route.MapPost("/signup", async (UserRequest req, AppDbContext context, IPasswordHasher<UserModel> hasher) =>
        {
            try
            {
                var errors = ValidationHelper.ValidateModel(req);
                if (errors.Any())
                    return ResponseHelper.BadRequest("Dados inválidos.", errors, new { Exemplo = "A palavra-passe deve ter no mínimo 6 dígitos." });

                if (await context.Users.AnyAsync(u => u.Email == req.Email))
                    return ResponseHelper.Conflict("Já existe um usuário com este e-mail.", new { req.Email });

                if (await context.Users.AnyAsync(u => u.Name == req.Name))
                    return ResponseHelper.Conflict("Já existe um usuário com este nome.", new { req.Name });

                var user = new UserModel
                {
                    Name = req.Name,
                    Email = req.Email,
                    Role = req.Role,
                    Active = true,
                    CreateDate = DateTime.UtcNow
                };

                user.Password = hasher.HashPassword(user, req.Password!);

                await context.Users.AddAsync(user);
                await context.SaveChangesAsync();

                return ResponseHelper.Created($"/user/{user.Id}", new
                {
                    user.Id,
                    user.Name,
                    user.Email,
                    user.Role,
                    user.Active,
                    user.CreateDate
                });
            }
            catch (Exception ex)
            {
                return ResponseHelper.ServerError(ex.Message);
            }
        }).WithSummary("Cadastra Usuários");

        route.MapPost("/signin", async (
            UserLoginRequest req,
            AppDbContext context,
            IPasswordHasher<UserModel> hasher,
            IConfiguration config) =>
            {
                try
                {
                    var user = await context.Users.FirstOrDefaultAsync(u => u.Email == req.Email);
                    if (user == null || !user.Active)
                        return ResponseHelper.NotFound("Usuário não encontrado ou desativado.", new { req.Email });

                    var result = hasher.VerifyHashedPassword(user, user.Password, req.Password);
                    if (result == PasswordVerificationResult.Failed)
                        return ResponseHelper.BadRequest("Credenciais incorretas.", null, new { Email = req.Email, Password = "********" });

                    string secretKey = config["Jwt:Key"] ?? "chave-secreta-superforte-com-32-caracteres!";
                    string token = JwtTokenService.GenerateToken(user.Email, user.Role, secretKey, user.Id, user.Name);

                    string refreshToken = JwtTokenService.GenerateRefreshToken();

                    var refreshModel = new RefreshTokenModel
                    {
                        UserId = user.Id,
                        Token = refreshToken,
                        Expiration = DateTime.UtcNow.AddDays(7) 
                    };

                    context.RefreshTokens.Add(refreshModel);
                    await context.SaveChangesAsync();

                    return ResponseHelper.Ok(new
                    {
                        Token = $"Bearer {token}",
                        RefreshToken = refreshToken
                    }, "Login realizado com sucesso.");
                }
                catch (Exception ex)
                {
                    return ResponseHelper.ServerError(ex.Message);
                }
            });

        route.MapPost("/refresh", async (AppDbContext context, RefreshRequest req, IConfiguration config) =>
        {
            var stored = await context.RefreshTokens
                .FirstOrDefaultAsync(r => r.Token == req.RefreshToken);

            if (stored == null)
                return ResponseHelper.BadRequest("Refresh token inválido.");

            if (stored.Revoked)
                return ResponseHelper.BadRequest("Refresh token já foi usado ou revogado.");

            if (stored.Expiration < DateTime.UtcNow)
            {
                stored.Revoked = true;
                await context.SaveChangesAsync();
                return ResponseHelper.BadRequest("Refresh token expirado.");
            }

            var user = await context.Users.FindAsync(stored.UserId);
            if (user == null)
                return ResponseHelper.NotFound("Usuário não encontrado.");

            string secretKey = config["Jwt:Key"]!;
            string newAccessToken = JwtTokenService.GenerateToken(user.Email, user.Role, secretKey, user.Id, user.Name);

            var newRefresh = new RefreshTokenModel
            {
                UserId = user.Id,
                Token = Guid.NewGuid().ToString(),
                Expiration = DateTime.UtcNow.AddDays(7)
            };

            stored.Revoked = true;
            context.RefreshTokens.Add(newRefresh);
            await context.SaveChangesAsync();

            return ResponseHelper.Ok(new
            {
                Token = $"Bearer {newAccessToken}",
                RefreshToken = newRefresh.Token
            }, "Token renovado com sucesso.");
        });

        route.MapGet("/profile", [Authorize] (HttpContext http) =>
        {
            try
            {
                var email = http.User.FindFirstValue(ClaimTypes.Name);
                var role = http.User.FindFirstValue(ClaimTypes.Role);
                var id = http.User.FindFirstValue("id");
                var name = http.User.FindFirstValue("name");

                return ResponseHelper.Ok(new
                {
                    Id = id,
                    Nome = name,
                    Email = email,
                    Role = role
                }, "Perfil obtido com sucesso.");
            }
            catch (Exception ex)
            {
                return ResponseHelper.ServerError(ex.Message);
            }
        }).WithSummary("Visualiza Perfil do Usuário Logado");

        route.MapPost("/logout", [Authorize] async (HttpContext http, AppDbContext context) =>
        {
            try
            {
                var jti = http.User.FindFirstValue(JwtRegisteredClaimNames.Jti);
                if (!string.IsNullOrEmpty(jti))
                {
                    context.RevokedTokens.Add(new RevokedTokenModel { Jti = jti });
                }

                var userId = http.User.FindFirstValue("id");
                if (!string.IsNullOrEmpty(userId))
                {
                    int id = int.Parse(userId);
                    var tokens = context.RefreshTokens.Where(t => t.UserId == id && !t.Revoked);
                    foreach (var t in tokens)
                        t.Revoked = true;
                }

                await context.SaveChangesAsync();
                return ResponseHelper.Ok(null, "Logout realizado com sucesso. Tokens revogados.");
            }
            catch (Exception ex)
            {
                return ResponseHelper.ServerError(ex.Message);
            }
        });

        route.MapGet("show", async (AppDbContext context) =>
        {
            try
            {
                var usuariosAtivos = await context.Users
                    .Where(u => u.Active)
                    .ToListAsync();

                return ResponseHelper.Ok(usuariosAtivos.Select(u => new
                {
                    u.Id,
                    u.Name,
                    u.Email,
                    u.Role,
                    u.CreateDate
                }), "Lista de usuários ativos obtida com sucesso.");
            }
            catch (Exception ex)
            {
                return ResponseHelper.ServerError(ex.Message);
            }
        }).WithSummary("Visualiza Usuários Ativos");

        route.MapPut("/{id:int}/update", async (int id, UserRequest req, AppDbContext context, IPasswordHasher<UserModel> hasher) =>
        {
            try
            {
                var errors = ValidationHelper.ValidateModel(req);
                if (errors.Any())
                    return ResponseHelper.BadRequest("Dados inválidos.", errors);

                var user = await context.Users.FirstOrDefaultAsync(u => u.Id == id);
                if (user == null)
                    return ResponseHelper.NotFound("Usuário não encontrado.", new { Id = id });

                if (await context.Users.AnyAsync(u => u.Email == req.Email && u.Id != id))
                    return ResponseHelper.Conflict("Já existe outro usuário com este e-mail.", new { req.Email });

                if (await context.Users.AnyAsync(u => u.Name == req.Name && u.Id != id))
                    return ResponseHelper.Conflict("Já existe outro usuário com este nome.", new { req.Name });

                user.Name = req.Name;
                user.Email = req.Email;
                user.Role = req.Role;
                user.Active = true;

                if (!string.IsNullOrWhiteSpace(req.Password))
                    user.Password = hasher.HashPassword(user, req.Password);

                await context.SaveChangesAsync();

                return ResponseHelper.Ok(new
                {
                    user.Id,
                    user.Name,
                    user.Email,
                    user.Role,
                    user.Active,
                    user.CreateDate
                }, "Usuário atualizado com sucesso.");
            }
            catch (Exception ex)
            {
                return ResponseHelper.ServerError(ex.Message);
            }
        }).WithSummary("Atualiza Usuário pelo ID");

        route.MapDelete("/{id:int}/delete", async (int id, AppDbContext context) =>
        {
            try
            {
                var user = await context.Users.FirstOrDefaultAsync(u => u.Id == id);
                if (user == null)
                    return ResponseHelper.NotFound("Usuário não encontrado.", new { Id = id });

                user.Active = false;
                await context.SaveChangesAsync();
                return ResponseHelper.Ok(new { user.Id, user.Email }, "Usuário desativado com sucesso.");
            }
            catch (Exception ex)
            {
                return ResponseHelper.ServerError(ex.Message);
            }
        }).WithSummary("Deleta Usuário pelo ID");
    }
}
