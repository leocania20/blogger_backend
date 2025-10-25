using System.IdentityModel.Tokens.Jwt;
using System.Security.Claims;
using System.Text;
using blogger_backend.Data;
using blogger_backend.Utils;
using Microsoft.EntityFrameworkCore;
using Microsoft.IdentityModel.Tokens;

public class JwtRefreshMiddleware
{
    private readonly RequestDelegate _next;
    private readonly IConfiguration _config;

    public JwtRefreshMiddleware(RequestDelegate next, IConfiguration config)
    {
        _next = next;
        _config = config;
    }

    public async Task InvokeAsync(HttpContext context, AppDbContext db)
    {
        var authHeader = context.Request.Headers["Authorization"].FirstOrDefault();
        if (string.IsNullOrEmpty(authHeader) || !authHeader.StartsWith("Bearer "))
        {
            await _next(context);
            return;
        }

        var token = authHeader.Substring("Bearer ".Length).Trim();
        var handler = new JwtSecurityTokenHandler();

        try
        {
            var key = Encoding.ASCII.GetBytes(_config["Jwt:Key"] ?? "chave-secreta-superforte-com-32-caracteres!");
            handler.ValidateToken(token, new TokenValidationParameters
            {
                ValidateIssuer = false,
                ValidateAudience = false,
                ValidateIssuerSigningKey = true,
                IssuerSigningKey = new SymmetricSecurityKey(key),
                ClockSkew = TimeSpan.Zero
            }, out var validatedToken);
            await _next(context);
        }
        catch (SecurityTokenExpiredException)
        {
            var refreshToken = context.Request.Headers["X-Refresh-Token"].FirstOrDefault();
            if (string.IsNullOrEmpty(refreshToken))
            {
                context.Response.StatusCode = 401;
                await context.Response.WriteAsJsonAsync(new { message = "Token expirado e sem refresh token." });
                return;
            }

            var stored = await db.RefreshTokens.FirstOrDefaultAsync(r => r.Token == refreshToken);
            if (stored == null || stored.Revoked || stored.Expiration < DateTime.UtcNow)
            {
                context.Response.StatusCode = 401;
                await context.Response.WriteAsJsonAsync(new { message = "Refresh token inválido ou expirado." });
                return;
            }

            var user = await db.Users.FindAsync(stored.UserId);
            if (user == null)
            {
                context.Response.StatusCode = 404;
                await context.Response.WriteAsJsonAsync(new { message = "Usuário não encontrado." });
                return;
            }
            var newAccessToken = JwtTokenService.GenerateToken(user.Email, user.Role, _config["Jwt:Key"]!, user.Id, user.Name);
            var newRefresh = JwtTokenService.GenerateRefreshToken();

            stored.Revoked = true;
            db.RefreshTokens.Add(new blogger_backend.Models.RefreshTokenModel
            {
                UserId = user.Id,
                Token = newRefresh,
                Expiration = DateTime.UtcNow.AddDays(7)
            });
            await db.SaveChangesAsync();

            context.Response.Headers["X-New-Access-Token"] = $"Bearer {newAccessToken}";
            context.Response.Headers["X-New-Refresh-Token"] = newRefresh;

            await _next(context);
        }
        catch (Exception)
        {
            context.Response.StatusCode = 401;
            await context.Response.WriteAsJsonAsync(new { message = "Token inválido." });
        }
    }
}
