using System.Diagnostics;
using blogger_backend.Data;
using blogger_backend.Models;

namespace blogger_backend.Middlewares
{
    public class AccessLogMiddleware
    {
        private readonly RequestDelegate _next;

        public AccessLogMiddleware(RequestDelegate next)
        {
            _next = next;
        }

        public async Task InvokeAsync(HttpContext context, AppDbContext db)
        {
            var stopwatch = Stopwatch.StartNew();

            try
            {
                await _next(context);
            }
            finally
            {
                stopwatch.Stop();

                var path = context.Request.Path.Value ?? "";
                var method = context.Request.Method;
                var ip = context.Connection.RemoteIpAddress?.ToString() ?? "desconhecido";
                var userAgent = context.Request.Headers["User-Agent"].ToString();

                string? userId = context.User.FindFirst("id")?.Value;
                string? userName = context.User.FindFirst("name")?.Value 
                                   ?? context.User.Identity?.Name;

                var log = new AccessLogModel
                {
                    Route = path,
                    Method = method,
                    IpAddress = ip,
                    UserAgent = userAgent,
                    UserId = userId,
                    UserName = userName,
                    DurationMs = stopwatch.Elapsed.TotalMilliseconds,
                    AccessDate = DateTime.UtcNow
                };

                db.AccessLogs.Add(log);
                await db.SaveChangesAsync();
            }
        }
    }
}
