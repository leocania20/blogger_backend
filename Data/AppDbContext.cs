using Microsoft.EntityFrameworkCore;
using blogger_backend.Models;

namespace blogger_backend.Data
{
    public class AppDbContext : DbContext
    {
        public AppDbContext(DbContextOptions<AppDbContext> options) : base(options)
        {
        }

        public DbSet<UserModel> Users { get; set; } = null!;
        public DbSet<AuthorModel> Authores { get; set; } = null!;
        public DbSet<CategoryModel> Categories { get; set; } = null!;
        public DbSet<SourceModel> Sources { get; set; } = null!;
        public DbSet<ArticlesModel> Articles { get; set; } = null!;
        public DbSet<CommentModel> Comments { get; set; } = null!;
        public DbSet<NewsletterModel> Newsletters { get; set; } = null!;
        public DbSet<NotificationModel> Notifications { get; set; } = null!;
        public DbSet<CustomizedResearchModel> CustomizedResearches { get; set; } = null!;
        public DbSet<AccessLogModel> AccessLogs { get; set; }


        protected override void OnModelCreating(ModelBuilder modelBuilder)
        {
            base.OnModelCreating(modelBuilder);

            modelBuilder.Entity<UserModel>()
                        .HasIndex(u => u.Email)
                        .IsUnique();
            
            modelBuilder.Entity<UserModel>()
            .HasIndex(u => u.Name)
            .IsUnique();
            
            modelBuilder.Entity<AuthorModel>()
                .HasIndex(a => a.Email)
                .IsUnique();

            modelBuilder.Entity<AuthorModel>()
                .HasIndex(a => a.Name)
                .IsUnique();

            modelBuilder.Entity<CategoryModel>()
                .HasIndex(c => c.Tag)
                .IsUnique();

            modelBuilder.Entity<CategoryModel>()
                .HasIndex(c => c.Name)
                .IsUnique();

            modelBuilder.Entity<SourceModel>()
                .HasIndex(f => f.Name)
                .IsUnique();

            modelBuilder.Entity<SourceModel>()
                .HasIndex(f => f.URL)
                .IsUnique();
            modelBuilder.Entity<ArticlesModel>()
                        .HasOne(a => a.Category)
                        .WithMany(c => c.Articles)
                        .HasForeignKey(a => a.CategoryId);

            modelBuilder.Entity<ArticlesModel>()
                        .HasOne(a => a.Author)
                        .WithMany(b => b.Articles)
                        .HasForeignKey(a => a.AuthorId);

            modelBuilder.Entity<ArticlesModel>()
                        .HasOne(a => a.Source)
                        .WithMany(f => f.Articles)
                        .HasForeignKey(a => a.SourceId)
                        .IsRequired(false);

            modelBuilder.Entity<CommentModel>()
                        .HasOne(c => c.User)
                        .WithMany(u => u.Comments)
                        .HasForeignKey(c => c.UserId);

            modelBuilder.Entity<CommentModel>()
                        .HasOne(c => c.Article)
                        .WithMany(a => a.Comment)
                        .HasForeignKey(c => c.ArticleId);

            modelBuilder.Entity<NotificationModel>()
                        .HasOne(n => n.User)
                        .WithMany(u => u.Notification)
                        .HasForeignKey(n => n.UserId);

            modelBuilder.Entity<NotificationModel>()
                        .HasOne(n => n.Article)
                        .WithMany(a => a.Notification)
                        .HasForeignKey(n => n.ArticleId)
                        .IsRequired(false);

            modelBuilder.Entity<AuthorModel>()
                        .HasOne(a => a.User)
                        .WithOne()
                        .HasForeignKey<AuthorModel>(a => a.UserId)
                        .OnDelete(DeleteBehavior.Cascade);
                        
            modelBuilder.Entity<CustomizedResearchModel>()
                .HasIndex(c => new { c.UserId, c.CategoryId })
                .IsUnique()
                .HasFilter("\"CategoryId\" IS NOT NULL");

            modelBuilder.Entity<CustomizedResearchModel>()
                .HasIndex(c => new { c.UserId, c.AuthorId })
                .IsUnique()
                .HasFilter("\"AuthorId\" IS NOT NULL");

            modelBuilder.Entity<CustomizedResearchModel>()
                .HasIndex(c => new { c.UserId, c.SourceId })
                .IsUnique()
                .HasFilter("\"SourceId\" IS NOT NULL");

                                        
            modelBuilder.Entity<CustomizedResearchModel>()
                .HasOne(c => c.User)
                .WithMany(u => u.CustomizedResearchModel)
                .HasForeignKey(p => p.UserId);

            modelBuilder.Entity<CustomizedResearchModel>()
                .HasOne(c => c.Category)
                .WithMany()
                .HasForeignKey(p => p.CategoryId)
                .OnDelete(DeleteBehavior.Restrict);

            modelBuilder.Entity<CustomizedResearchModel>()
                .HasOne(c => c.Author)
                .WithMany()
                .HasForeignKey(p => p.AuthorId)
                .OnDelete(DeleteBehavior.Restrict);

            modelBuilder.Entity<CustomizedResearchModel>()
                .HasOne(c => c.Source)
                .WithMany()
                .HasForeignKey(p => p.SourceId)
                .OnDelete(DeleteBehavior.Restrict);
                    
        }
    }
}
