using blogger_backend.Routes;
using blogger_backend.Models;
using blogger_backend.Data;
using Microsoft.EntityFrameworkCore;
using Microsoft.AspNetCore.Authentication.JwtBearer;
using Microsoft.IdentityModel.Tokens;
using Microsoft.AspNetCore.Identity;
using System.Text;

var builder = WebApplication.CreateBuilder(args);

builder.Services.AddEndpointsApiExplorer();
builder.Services.AddSwaggerGen();

// 📌 Caminho fixo para o banco de dados SQLite
var dbPath = Path.Combine(AppContext.BaseDirectory, "blogger_backend.db");

// Garante que a pasta Data exista (mesmo no Render ou Docker)
Directory.CreateDirectory(Path.GetDirectoryName(dbPath)!);

// Configuração do DbContext
builder.Services.AddDbContext<AppDbContext>(options =>
{
    var basePath = AppContext.BaseDirectory;
    var dbPath = Path.Combine(basePath, "blogger_backend.db");
    options.UseSqlite($"Data Source={dbPath}");
});

// PasswordHasher
builder.Services.AddScoped<IPasswordHasher<UsuarioModel>, PasswordHasher<UsuarioModel>>();

// CORS
builder.Services.AddCors(options =>
{
    options.AddPolicy("AllowAll",
        policy => policy.AllowAnyOrigin()
                        .AllowAnyMethod()
                        .AllowAnyHeader());
});

// JWT Config
var key = builder.Configuration["Jwt:Key"] ?? "chave-secreta-superforte";
builder.Services.AddAuthentication(options =>
{
    options.DefaultAuthenticateScheme = JwtBearerDefaults.AuthenticationScheme;
    options.DefaultChallengeScheme = JwtBearerDefaults.AuthenticationScheme;
})
.AddJwtBearer(options =>
{
    options.RequireHttpsMetadata = false;
    options.SaveToken = true;
    options.TokenValidationParameters = new TokenValidationParameters
    {
        ValidateIssuer = false,
        ValidateAudience = false,
        ValidateLifetime = true,
        ValidateIssuerSigningKey = true,
        IssuerSigningKey = new SymmetricSecurityKey(Encoding.ASCII.GetBytes(key))
    };
});

builder.Services.AddAuthorization();

var app = builder.Build();

// Swagger
app.UseSwagger();
app.UseSwaggerUI(options =>
{
    options.SwaggerEndpoint("/swagger/v1/swagger.json", "Blogger API v1");
    options.RoutePrefix = string.Empty;
});

// Migrações automáticas
using (var scope = app.Services.CreateScope())
{
    var db = scope.ServiceProvider.GetRequiredService<AppDbContext>();
    db.Database.EnsureCreated();
}

// Middlewares
app.UseCors("AllowAll");
app.UseAuthentication();
app.UseAuthorization();
app.UseStaticFiles();

// Rotas
app.UsuarioRoutes(builder.Configuration);
app.ArtigoRoutes();
app.AutorRoutes();
app.CategoriaRoutes();
app.FonteRoutes();
app.ComentarioRoutes();
app.NewsletterRoutes();
app.NotificacaoRoutes();
app.PesquisaCustomizadaRoutes();

app.Run();
