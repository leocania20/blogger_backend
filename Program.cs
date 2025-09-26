using blogger_backend.Routes;
using blogger_backend.Models;
using blogger_backend.Data;
using Microsoft.EntityFrameworkCore;

var builder = WebApplication.CreateBuilder(args);

// Forçar ambiente Development dentro do Docker
builder.Environment.EnvironmentName = "Development";

// URLs para escutar em todas as interfaces (HTTP)
builder.WebHost.UseUrls("http://0.0.0.0:5000");

// Add services
builder.Services.AddEndpointsApiExplorer();
builder.Services.AddSwaggerGen();

// Registrar DbContext (SQLite)
builder.Services.AddDbContext<AppDbContext>(options =>
    options.UseSqlite("Data Source=blogger_backend.db"));

// Se precisar de scoped services extras
builder.Services.AddScoped<AppDbContext>();

var app = builder.Build();

// Configure Swagger (vai funcionar porque forçamos Development)
app.UseSwagger();
app.UseSwaggerUI(options =>
{
    options.SwaggerEndpoint("/swagger/v1/swagger.json", "Blogger API v1");
    options.RoutePrefix = string.Empty; // Swagger na raiz
});
using (var scope = app.Services.CreateScope())
{
    var db = scope.ServiceProvider.GetRequiredService<AppDbContext>();
    db.Database.Migrate(); // Cria todas as tabelas que não existem
}


// Registrar Routes
app.UsuarioRoutes();
app.ArtigoRoutes();
app.AutorRoutes();
app.CategoriaRoutes();
app.FonteRoutes();
app.ComentarioRoutes();
app.NewsletterRoutes();
app.NotificacaoRoutes();

// Desabilitar HTTPS temporariamente para Docker
// app.UseHttpsRedirection(); // comentar enquanto estiver testando no container

app.Run();
