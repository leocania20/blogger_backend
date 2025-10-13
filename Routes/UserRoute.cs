using Microsoft.EntityFrameworkCore;
using blogger_backend.Models;
using blogger_backend.Data;
using blogger_backend.Utils;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Authorization;
using System.Security.Claims;

namespace blogger_backend.Routes;

public static class UserRoute
{
    public static void UserRoutes(this WebApplication app, IConfiguration config)
    {
        var route = app.MapGroup("/user").WithTags("Users");

        route.MapPost("/signup", async (UserRequest req, AppDbContext context, IPasswordHasher<UserModel> hasher) =>
        {
            var errors = ValidationHelper.ValidateModel(req);
            if (errors.Any())
                return Results.BadRequest(new { Error = "A palavra passe deve ter no mínimo 6 digitos" });

            if (await context.Users.AnyAsync(u => u.Email == req.Email))
                return Results.BadRequest(new { Error = "Já existe um usuário com este e-mail." });

            if (await context.Users.AnyAsync(u => u.Name == req.Name))
                return Results.BadRequest(new { Error = "Já existe um usuário com este nome." });

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

            return Results.Created($"/user/{user.Id}", new {
                user.Id,
                user.Name,
                user.Email,
                user.Role,
                user.Active,
                user.CreateDate
            });
        });

        route.MapPost("/signin", async (
            UserLoginRequest req,
            AppDbContext context,
            IPasswordHasher<UserModel> hasher) =>
        {
            var user = await context.Users.FirstOrDefaultAsync(u => u.Email == req.Email);
            if (user == null || !user.Active)
                return Results.Unauthorized();

            var result = hasher.VerifyHashedPassword(user, user.Password, req.Password);
            if (result == PasswordVerificationResult.Failed)
                return Results.Unauthorized();

            string secretKey = config["Jwt:Key"] ?? "chave-secreta-superforte-com-32-caracteres!";
            string token = JwtTokenService.GenerateToken(user.Email, user.Role, secretKey, user.Id, user.Name);

            return Results.Ok(new { Token = $"Bearer {token}" });
        });

        route.MapGet("/profile", [Authorize] (HttpContext http) =>
        {
            var email = http.User.FindFirstValue(ClaimTypes.Name);
            var role  = http.User.FindFirstValue(ClaimTypes.Role);
            var id    = http.User.FindFirstValue("id");
            var name  = http.User.FindFirstValue("name");

            return Results.Ok(new
            {
                Email = email,
                Nome=name
            });
        });


        route.MapGet("show", async (AppDbContext context) =>
        {
            var usuariosAtivos = await context.Users
            .Where(u => u.Active) 
            .ToListAsync();

            return Results.Ok(usuariosAtivos.Select(u => new
            {
                u.Id,
                u.Name,
                u.Email,
                u.Role,
                u.CreateDate
            }));
        });

        route.MapPut("/{id:int}/update", async (int id, UserRequest req, AppDbContext context, IPasswordHasher<UserModel> hasher) =>
        {
            var errors = ValidationHelper.ValidateModel(req);
            if (errors.Any())
                return Results.BadRequest(new { Errors = errors });

            var user = await context.Users.FirstOrDefaultAsync(u => u.Id == id);
            if (user == null)
                return Results.NotFound(new { Error = "Usuário não encontrado." });

            if (await context.Users.AnyAsync(u => u.Email == req.Email && u.Id != id))
                return Results.BadRequest(new { Error = "Já existe outro usuário com este e-mail." });

            if (await context.Users.AnyAsync(u => u.Name == req.Name && u.Id != id))
                return Results.BadRequest(new { Error = "Já existe outro usuário com este nome." });

            user.Name = req.Name;
            user.Email = req.Email;
            user.Role = req.Role;
            user.Active = true;

            if (!string.IsNullOrWhiteSpace(req.Password))
                user.Password = hasher.HashPassword(user, req.Password);

            await context.SaveChangesAsync();

            return Results.Ok(new {
                user.Id,
                user.Name,
                user.Email,
                user.Role,
                user.Active,
                user.CreateDate
            });
        });


        route.MapDelete("/{id:int}/delete", async (int id, AppDbContext context) =>
        {
            var user = await context.Users.FirstOrDefaultAsync(u => u.Id == id);
            if (user == null) return Results.NotFound();

            user.Active = false;
            await context.SaveChangesAsync();
            return Results.Ok(new { Message = "Usuário desativado com sucesso." });
        });
    }
}
