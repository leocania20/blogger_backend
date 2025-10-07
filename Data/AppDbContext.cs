using Microsoft.EntityFrameworkCore;
using blogger_backend.Models;

namespace blogger_backend.Data
{
    public class AppDbContext : DbContext
    {
        public AppDbContext(DbContextOptions<AppDbContext> options) : base(options)
        {
        }

        public DbSet<UsuarioModel> Usuarios { get; set; } = null!;
        public DbSet<AutorModel> Autores { get; set; } = null!;
        public DbSet<CategoriaModel> Categorias { get; set; } = null!;
        public DbSet<FonteModel> Fontes { get; set; } = null!;
        public DbSet<ArtigoModel> Artigos { get; set; } = null!;
        public DbSet<ComentarioModel> Comentarios { get; set; } = null!;
        public DbSet<NewsletterModel> Newsletters { get; set; } = null!;
        public DbSet<NotificacaoModel> Notificacoes { get; set; } = null!;
        public DbSet<PesquisaCustomizadaModel> PesquisasCustomizadas { get; set; } = null!;

        protected override void OnModelCreating(ModelBuilder modelBuilder)
        {
            base.OnModelCreating(modelBuilder);

            modelBuilder.Entity<UsuarioModel>()
                        .HasIndex(u => u.Email)
                        .IsUnique();
            
            modelBuilder.Entity<UsuarioModel>()
            .HasIndex(u => u.Nome)
            .IsUnique();
            
            modelBuilder.Entity<AutorModel>()
                .HasIndex(a => a.Email)
                .IsUnique();

            modelBuilder.Entity<AutorModel>()
                .HasIndex(a => a.Nome)
                .IsUnique();

            modelBuilder.Entity<CategoriaModel>()
                .HasIndex(c => c.Slug)
                .IsUnique();

            modelBuilder.Entity<CategoriaModel>()
                .HasIndex(c => c.Nome)
                .IsUnique();

            modelBuilder.Entity<FonteModel>()
                .HasIndex(f => f.Nome)
                .IsUnique();

            modelBuilder.Entity<FonteModel>()
                .HasIndex(f => f.URL)
                .IsUnique();
            modelBuilder.Entity<ArtigoModel>()
                        .HasOne(a => a.Categoria)
                        .WithMany(c => c.Artigos)
                        .HasForeignKey(a => a.CategoriaId);

            modelBuilder.Entity<ArtigoModel>()
                        .HasOne(a => a.Autor)
                        .WithMany(b => b.Artigos)
                        .HasForeignKey(a => a.AutorId);

            modelBuilder.Entity<ArtigoModel>()
                        .HasOne(a => a.Fonte)
                        .WithMany(f => f.Artigos)
                        .HasForeignKey(a => a.FonteId)
                        .IsRequired(false);

            modelBuilder.Entity<ComentarioModel>()
                        .HasOne(c => c.Usuario)
                        .WithMany(u => u.Comentarios)
                        .HasForeignKey(c => c.UsuarioId);

            modelBuilder.Entity<ComentarioModel>()
                        .HasOne(c => c.Artigo)
                        .WithMany(a => a.Comentarios)
                        .HasForeignKey(c => c.ArtigoId);

            modelBuilder.Entity<NotificacaoModel>()
                        .HasOne(n => n.Usuario)
                        .WithMany(u => u.Notificacoes)
                        .HasForeignKey(n => n.UsuarioId);

            modelBuilder.Entity<NotificacaoModel>()
                        .HasOne(n => n.Artigo)
                        .WithMany(a => a.Notificacoes)
                        .HasForeignKey(n => n.ArtigoId)
                        .IsRequired(false);
            
            modelBuilder.Entity<PesquisaCustomizadaModel>()
                        .HasIndex(p => new { p.UsuarioId, p.CategoriaId, p.AutorId, p.FonteId })
                        .IsUnique();
                                        
            modelBuilder.Entity<PesquisaCustomizadaModel>()
                .HasOne(p => p.Usuario)
                .WithMany(u => u.PesquisasCustomizadas)
                .HasForeignKey(p => p.UsuarioId);

            modelBuilder.Entity<PesquisaCustomizadaModel>()
                .HasOne(p => p.Categoria)
                .WithMany()
                .HasForeignKey(p => p.CategoriaId)
                .OnDelete(DeleteBehavior.Restrict);

            modelBuilder.Entity<PesquisaCustomizadaModel>()
                .HasOne(p => p.Autor)
                .WithMany()
                .HasForeignKey(p => p.AutorId)
                .OnDelete(DeleteBehavior.Restrict);

            modelBuilder.Entity<PesquisaCustomizadaModel>()
                .HasOne(p => p.Fonte)
                .WithMany()
                .HasForeignKey(p => p.FonteId)
                .OnDelete(DeleteBehavior.Restrict);
                    
        }
    }
}
