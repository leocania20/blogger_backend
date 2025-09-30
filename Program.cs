using blogger_backend.Routes;
using blogger_backend.Models;
using blogger_backend.Data;
using Microsoft.EntityFrameworkCore;
using Microsoft.AspNetCore.Authentication.JwtBearer;
using Microsoft.IdentityModel.Tokens;
using Microsoft.AspNetCore.Identity;
using System.Text;

var builder = WebApplication.CreateBuilder(args);

// === 1. REGISTRO DE SERVIÇOS (Dependency Injection) ===

// Serviços base
builder.Services.AddEndpointsApiExplorer();
builder.Services.AddSwaggerGen();

// Registrar DbContext (SQLite)
builder.Services.AddDbContext<AppDbContext>(options =>
    options.UseSqlite(builder.Configuration.GetConnectionString("DefaultConnection") 
        ?? "Data Source=blogger_backend.db"));

// Registrar o serviço de hasher (antes do Build)
builder.Services.AddScoped<IPasswordHasher<UsuarioModel>, PasswordHasher<UsuarioModel>>();

// Para permitir o acesso de qualquer origem (CORS)
builder.Services.AddCors(options =>
{
    options.AddPolicy("AllowAll",
        policy => policy.AllowAnyOrigin()
                        .AllowAnyMethod()
                        .AllowAnyHeader());
});

// Configuração JWT (Autenticação)
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

// === 2. MIDDLEWARE (Pipeline de Requisição) ===

// Configuração Swagger/OpenAPI
app.UseSwagger();
app.UseSwaggerUI(options =>
{
    options.SwaggerEndpoint("/swagger/v1/swagger.json", "Blogger API v1");
    options.RoutePrefix = string.Empty; // Swagger na raiz
});

// Executa Migrações do Banco de Dados na inicialização
using (var scope = app.Services.CreateScope())
{
    var db = scope.ServiceProvider.GetRequiredService<AppDbContext>();
    db.Database.Migrate(); // Cria todas as tabelas que não existem
   }

// Configuração de CORS, Autenticação e Autorização (a ordem é importante: CORS -> Auth -> Policy)
app.UseCors("AllowAll"); 
app.UseAuthentication();
app.UseAuthorization();
app.UseStaticFiles(); // já serve os arquivos da pasta wwwroot


// Desabilitar HTTPS temporariamente para Docker
// app.UseHttpsRedirection(); 

// Registrar Rotas (Endpoints)
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
