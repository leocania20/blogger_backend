using System.IdentityModel.Tokens.Jwt;
using blogger_backend.Data;

public class RevokedTokenMiddleware
{
    private readonly RequestDelegate _next;

    public RevokedTokenMiddleware(RequestDelegate next)
    {
        _next = next;
    }

    public async Task InvokeAsync(HttpContext context, AppDbContext db)
    {
        var authHeader = context.Request.Headers["Authorization"].FirstOrDefault();
        if (!string.IsNullOrEmpty(authHeader) && authHeader.StartsWith("Bearer "))
        {
            var token = authHeader.Substring("Bearer ".Length).Trim();
            var handler = new JwtSecurityTokenHandler();

            if (handler.CanReadToken(token))
            {
                var jwt = handler.ReadJwtToken(token);
                var jti = jwt.Claims.FirstOrDefault(c => c.Type == JwtRegisteredClaimNames.Jti)?.Value;

                if (jti != null && db.RevokedTokens.Any(t => t.Jti == jti))
                {
                    context.Response.StatusCode = 401;
                    await context.Response.WriteAsJsonAsync(new { message = "Token revogado. Faça login novamente." });
                    return;
                }
            }
        }

        await _next(context);
    }
}
