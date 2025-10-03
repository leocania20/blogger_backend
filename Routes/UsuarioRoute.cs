using Microsoft.EntityFrameworkCore;
using blogger_backend.Models;
using blogger_backend.Data;
using blogger_backend.Utils;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Authorization;
using System.Security.Claims;

namespace blogger_backend.Routes;

public static class UsuarioRoute
{
    public static void UsuarioRoutes(this WebApplication app, IConfiguration config)
    {
        var route = app.MapGroup("/usuario");

        // Post
        route.MapPost("", async (UsuarioRequest req, AppDbContext context, IPasswordHasher<UsuarioModel> hasher) =>
        {
            if (await context.Usuarios.AnyAsync(u => u.Email == req.Email))
                return Results.BadRequest(new { Error = "Já existe um usuário com este e-mail." });

            var usuario = new UsuarioModel
            {
                Nome = req.Nome,
                Email = req.Email,
                Role = req.Role,
                Ativo = true,
                DataCadastro = DateTime.UtcNow
            };

            usuario.SenhaHash = hasher.HashPassword(usuario, req.Senha);

            await context.Usuarios.AddAsync(usuario);
            await context.SaveChangesAsync();

            return Results.Created($"/usuario/{usuario.Id}", new {
                usuario.Id,
                usuario.Nome,
                usuario.Email,
                usuario.Role,
                usuario.Ativo,
                usuario.DataCadastro
            });
        });

        // Login
        route.MapPost("/login", async (
            UsuarioLoginRequest req,
            AppDbContext context,
            IPasswordHasher<UsuarioModel> hasher) =>
        {
            var usuario = await context.Usuarios.FirstOrDefaultAsync(u => u.Email == req.Email);
            if (usuario == null || !usuario.Ativo)
                return Results.Unauthorized();

            var result = hasher.VerifyHashedPassword(usuario, usuario.SenhaHash, req.Senha);
            if (result == PasswordVerificationResult.Failed)
                return Results.Unauthorized();

            string secretKey = config["Jwt:Key"] ?? "chave-secreta-superforte-com-32-caracteres!";
            string token = JwtTokenService.GenerateToken(usuario.Email, usuario.Role, secretKey, usuario.Id, usuario.Nome);

            return Results.Ok(new { Token = $"Bearer {token}" });
        });

        // Novo endpoint: pegar dados do usuário logado via Token
        route.MapGet("/me", [Authorize] (HttpContext http) =>
        {
            var email = http.User.FindFirstValue(ClaimTypes.Name);
            var role  = http.User.FindFirstValue(ClaimTypes.Role);
            var id    = http.User.FindFirstValue("id");
            var name  = http.User.FindFirstValue("name");

            return Results.Ok(new
            {
                Id = id,
                Email = email,
                Role = role
            });
        });

        // Listar usuários (apenas admin deveria usar isso)
        route.MapGet("", async (AppDbContext context) =>
        {
            var usuarios = await context.Usuarios.ToListAsync();
            return Results.Ok(usuarios.Select(u => new {
                u.Id, u.Nome, u.Email, u.Role, u.DataCadastro, u.Ativo
            }));
        });

        // Put
        route.MapPut("/{id:int}", async (int id, UsuarioRequest req, AppDbContext context, IPasswordHasher<UsuarioModel> hasher) =>
        {
            var usuario = await context.Usuarios.FirstOrDefaultAsync(u => u.Id == id);
            if (usuario == null) return Results.NotFound();

            usuario.Nome = req.Nome;
            usuario.Email = req.Email;
            usuario.Role = req.Role;
            usuario.Ativo = true;

            if (!string.IsNullOrWhiteSpace(req.Senha))
                usuario.SenhaHash = hasher.HashPassword(usuario, req.Senha);

            await context.SaveChangesAsync();

            return Results.Ok(new {
                usuario.Id,
                usuario.Nome,
                usuario.Email,
                usuario.Role,
                usuario.Ativo,
                usuario.DataCadastro
            });
        });

        // Delete
        route.MapDelete("/{id:int}", async (int id, AppDbContext context) =>
        {
            var usuario = await context.Usuarios.FirstOrDefaultAsync(u => u.Id == id);
            if (usuario == null) return Results.NotFound();

            usuario.Ativo = false;
            await context.SaveChangesAsync();
            return Results.Ok(new { Message = "Usuário desativado com sucesso." });
        });
    }
}
