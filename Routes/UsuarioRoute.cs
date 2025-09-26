using Microsoft.EntityFrameworkCore;
using blogger_backend.Models;
using blogger_backend.Data;

namespace blogger_backend.Routes;

public static class UsuarioRoute
{
    public static void UsuarioRoutes(this WebApplication app)
    {
        var route = app.MapGroup("/usuario");

        // POST usando UsuarioRequest
        route.MapPost("", async (UsuarioRequest req, AppDbContext context) =>
        {
            var usuario = new UsuarioModel
            {
                Nome = req.Nome,
                Email = req.Email,
                SenhaHash = HashPassword(req.Senha), // proteger senha
                Role = req.Role,
                Ativo = true,
                DataCadastro = DateTime.UtcNow
            };

            await context.Usuarios.AddAsync(usuario);
            await context.SaveChangesAsync();
            return Results.Created($"/usuario/{usuario.Id}", usuario);
        });

        // GET
        route.MapGet("", async (AppDbContext context) =>
        {
            var usuarios = await context.Usuarios.ToListAsync();
            return Results.Ok(usuarios.Select(u => new {
                u.Id, u.Nome, u.Email, u.Role, u.DataCadastro, u.Ativo
            }));
        });

        // PUT
        route.MapPut("/{id:int}", async (int id, UsuarioRequest req, AppDbContext context) =>
        {
            var usuario = await context.Usuarios.FirstOrDefaultAsync(u => u.Id == id);
            if (usuario == null) return Results.NotFound();

            usuario.Nome = req.Nome;
            usuario.Email = req.Email;
            usuario.Role = req.Role;
            usuario.Ativo = true; // manter ativo ao atualizar
            await context.SaveChangesAsync();
            return Results.Ok(usuario);
        });

        // DELETE
        route.MapDelete("/{id:int}", async (int id, AppDbContext context) =>
        {
            var usuario = await context.Usuarios.FirstOrDefaultAsync(u => u.Id == id);
            if (usuario == null) return Results.NotFound();

            usuario.Ativo = false; // Soft delete
            await context.SaveChangesAsync();
            return Results.Ok();
        });
    }

    // Exemplo de função de hash de senha
    private static string HashPassword(string senha)
    {
        // Aqui você pode usar um hash real, ex: BCrypt
        return Convert.ToBase64String(System.Text.Encoding.UTF8.GetBytes(senha));
    }
}
