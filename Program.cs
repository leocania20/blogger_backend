using blogger_backend.Utils;
using blogger_backend.Routes;
using blogger_backend.Models;
using blogger_backend.Data;
using blogger_backend.Middlewares;
using Microsoft.EntityFrameworkCore;
using Microsoft.AspNetCore.Authentication.JwtBearer;
using Microsoft.IdentityModel.Tokens;
using Microsoft.AspNetCore.Identity;
using Microsoft.Extensions.FileProviders;
using Microsoft.OpenApi.Models;
using Microsoft.AspNetCore.Mvc;
using System.Security.Claims;
using System.Text;
using System.Text.Json.Serialization;

var builder = WebApplication.CreateBuilder(args);

builder.Configuration.AddEnvironmentVariables();

string GetEnvOrConfig(string key)
{
    var value = builder.Configuration[key];
    if (!string.IsNullOrEmpty(value) && value.StartsWith("env:"))
    {
        var envKey = value.Replace("env:", "");
        return Environment.GetEnvironmentVariable(envKey) ?? "";
    }
    return value ?? "";
}

// -------------------------------
// 🔧 Configuração do Swagger
// -------------------------------
builder.Services.AddEndpointsApiExplorer();
builder.Services.AddSwaggerGen(c =>
{
    c.SwaggerDoc("v1", new OpenApiInfo { Title = "Blogger API", Version = "v1" });

    c.AddSecurityDefinition("Bearer", new OpenApiSecurityScheme
    {
        Name = "Authorization",
        Type = SecuritySchemeType.ApiKey,
        Scheme = "Bearer",
        BearerFormat = "JWT",
        In = ParameterLocation.Header,
        Description = "Insira o token JWT assim: Bearer {seu_token}"
    });

    c.AddSecurityRequirement(new OpenApiSecurityRequirement
    {
        {
            new OpenApiSecurityScheme
            {
                Reference = new OpenApiReference
                {
                    Type = ReferenceType.SecurityScheme,
                    Id = "Bearer"
                }
            },
            Array.Empty<string>()
        }
    });
});

// -------------------------------
// 🗄️ Configuração do Banco de Dados (PostgreSQL)
// -------------------------------
string connectionString;
var databaseUrl = Environment.GetEnvironmentVariable("DATABASE_URL");

if (!string.IsNullOrEmpty(databaseUrl))
{
    var uri = new Uri(databaseUrl);
    var userInfo = uri.UserInfo.Split(':');
    var port = uri.Port > 0 ? uri.Port : 5432;

    connectionString =
        $"Host={uri.Host};Port={port};Username={userInfo[0]};Password={userInfo[1]};Database={uri.AbsolutePath.TrimStart('/')};SSL Mode=Require;Trust Server Certificate=true";

    Console.WriteLine($"[INFO] Conectando ao PostgreSQL no Render: {uri.Host}:{port}");
}
else
{
    connectionString = builder.Configuration.GetConnectionString("DefaultConnection")
                       ?? throw new InvalidOperationException("Connection string 'DefaultConnection' não foi encontrada.");
    Console.WriteLine("[INFO] Usando connection string local.");
}

builder.Services.AddDbContext<AppDbContext>(options =>
    options.UseNpgsql(connectionString)
);

// -------------------------------
// ⚙️ Configuração JSON
// -------------------------------
builder.Services.Configure<Microsoft.AspNetCore.Http.Json.JsonOptions>(options =>
{
    options.SerializerOptions.ReferenceHandler = ReferenceHandler.IgnoreCycles;
    options.SerializerOptions.WriteIndented = true;
});

// -------------------------------
// 🛠️ Serviços (injeções)
// -------------------------------
builder.Services.AddScoped<IPasswordHasher<UserModel>, PasswordHasher<UserModel>>();
builder.Services.AddScoped<SendGridEmailServices>();

// -------------------------------
// 🌍 CORS
// -------------------------------
builder.Services.AddCors(options =>
{
    options.AddPolicy("AllowAll", policy =>
        policy.AllowAnyOrigin()
              .AllowAnyMethod()
              .AllowAnyHeader());
});

// -------------------------------
// 🚫 Desativar validação de antiforgery
// -------------------------------
builder.Services.AddControllers(options =>
{
    options.Filters.Add(new IgnoreAntiforgeryTokenAttribute());
});

// -------------------------------
// 🔐 JWT
// -------------------------------
var jwtKey = GetEnvOrConfig("Jwt:Key");
if (string.IsNullOrEmpty(jwtKey))
    jwtKey = "chave-secreta-superforte"; // fallback local

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
        IssuerSigningKey = new SymmetricSecurityKey(Encoding.ASCII.GetBytes(jwtKey))
    };
});

// -------------------------------
// 🔒 Autorização
// -------------------------------
builder.Services.AddAuthorization(options =>
{
    options.AddPolicy("Admin", policy =>
        policy.RequireClaim(ClaimTypes.Role, "admin"));

    options.AddPolicy("Painel do Admin", policy =>
        policy.RequireClaim(ClaimTypes.Role, "admin", "SuperAdmin"));
});

// -------------------------------
// 🚀 Construção do app
// -------------------------------
var app = builder.Build();

if (app.Environment.IsDevelopment())
{
    app.UseDeveloperExceptionPage();
}

// -------------------------------
// 📘 Swagger
// -------------------------------
app.UseSwagger();
app.UseSwaggerUI(options =>
{
    options.SwaggerEndpoint("/swagger/v1/swagger.json", "Blogger API v1");
    options.RoutePrefix = string.Empty;
});

// -------------------------------
// 🖼️ Arquivos estáticos
// -------------------------------
app.UseStaticFiles(new StaticFileOptions
{
    FileProvider = new PhysicalFileProvider(
        Path.Combine(Directory.GetCurrentDirectory(), "wwwroot")
    ),
    RequestPath = ""
});

// -------------------------------
// 🚦 Pipeline
// -------------------------------
app.UseRouting();
app.UseCors("AllowAll");
app.UseAuthentication();
app.UseAuthorization();

app.UseMiddleware<AccessLogMiddleware>();

// -------------------------------
// 🧭 Rotas (endpoints)
// -------------------------------
app.UserRoutes(builder.Configuration);
app.MonitoringRoutes();
app.ArticlesRoutes();      // ✅ Aqui está a rota de criação do artigo
app.AuthorRoutes();
app.CategoryRoutes();
app.SourceRoute();
app.CommentRoutes();
app.NewsletterRoute();
app.NotificationRoutes();
app.PesquisaCustomizadaRoutes();

app.UseMiddleware<RevokedTokenMiddleware>();

// -------------------------------
// 🗃️ Migrações automáticas
// -------------------------------
using (var scope = app.Services.CreateScope())
{
    var db = scope.ServiceProvider.GetRequiredService<AppDbContext>();
    db.Database.Migrate();
}

app.Run();
