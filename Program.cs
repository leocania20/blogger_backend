using blogger_backend.Routes;
using blogger_backend.Models;
using blogger_backend.Data;
using Microsoft.EntityFrameworkCore;
using Microsoft.AspNetCore.Authentication.JwtBearer;
using Microsoft.IdentityModel.Tokens;
using Microsoft.AspNetCore.Identity;
using System.Text;
using Microsoft.OpenApi.Models;

var builder = WebApplication.CreateBuilder(args);

builder.Services.AddEndpointsApiExplorer();

// Configuração do Swagger/OpenAPI (Correto, sem alterações)
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

// ************************************************
// CORREÇÃO APLICADA AQUI PARA ROBUSTEZ DA CONEXÃO
// ************************************************
var connectionString = builder.Configuration.GetConnectionString("DefaultConnection");

if (!string.IsNullOrEmpty(connectionString) && connectionString.StartsWith("postgresql://"))
{
    // O Render geralmente fornece a string em formato URL (postgresql://...)
    // O Npgsql pode interpretar a URL, mas precisamos garantir o Ssl Mode=Require
    
    // Converte a URL para URI para extrair componentes
    var uri = new Uri(connectionString);
    var db = uri.AbsolutePath.Trim('/');
    var userInfo = uri.UserInfo.Split(':');
    var user = userInfo[0];
    var password = userInfo.Length > 1 ? userInfo[1] : string.Empty;

    // Recria a string no formato Key=Value para maior compatibilidade e força SSL
    connectionString = $"Host={uri.Host};Port={uri.Port};Database={db};Username={user};Password={password};Ssl Mode=Require;Trust Server Certificate=true";
}
// Se já estiver em Key=Value, garante que tenha Ssl Mode
else if (!string.IsNullOrEmpty(connectionString) && !connectionString.Contains("Ssl Mode="))
{
    connectionString += ";Ssl Mode=Require;Trust Server Certificate=true";
}


builder.Services.AddDbContext<AppDbContext>(options =>
    options.UseNpgsql(connectionString)
);
// ************************************************

builder.Services.AddScoped<IPasswordHasher<UsuarioModel>, PasswordHasher<UsuarioModel>>();

// Configuração de CORS (Correto, sem alterações)
builder.Services.AddCors(options =>
{
    options.AddPolicy("AllowAll",
        policy => policy.AllowAnyOrigin()
                        .AllowAnyMethod()
                        .AllowAnyHeader());
});

// Configuração de JWT (Correto, sem alterações)
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
        // Usando Encoding.UTF8 para maior compatibilidade (melhor prática)
        IssuerSigningKey = new SymmetricSecurityKey(Encoding.UTF8.GetBytes(key)) 
    };
});

builder.Services.AddAuthorization();

var app = builder.Build();

// Middlewares (Corretos, sem alterações)
app.UseSwagger();
app.UseSwaggerUI(options =>
{
    options.SwaggerEndpoint("/swagger/v1/swagger.json", "Blogger API v1");
    options.RoutePrefix = string.Empty;
});

// Aplicação de Migrações (Correta, sem alterações)
using (var scope = app.Services.CreateScope())
{
    var db = scope.ServiceProvider.GetRequiredService<AppDbContext>();
    // Esta linha pode causar falha se a string de conexão estiver errada
    db.Database.Migrate(); 
}

app.UseCors("AllowAll"); 
app.UseAuthentication();
app.UseAuthorization();
app.UseStaticFiles(); 

// Mapeamento de Rotas (Correto, sem alterações)
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