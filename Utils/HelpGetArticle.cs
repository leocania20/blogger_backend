using Microsoft.EntityFrameworkCore;
using blogger_backend.Data;

namespace blogger_backend.Utils
{
    public static class HelpGetArticle
    {
        public static async Task<object> GetArticle(
            AppDbContext context,
            int currentPage,
            int pageSize,
            int? id = null,
            string? title = null,
            List<int>? categoriesIds = null,
            List<int>? authorsIds = null,
            List<int>? sourcesIds = null,
            DateTime? date = null,
            bool withText = false)
        {
            if (currentPage <= 0)
            {
                return new
                {
                    success = false,
                    message = "O número da página deve ser maior que zero.",
                    example = new
                    {
                        currentPage = 1,
                        pageSize = 6,
                        title = "Exemplo de título",
                        categoriesIds = new List<int> { 1, 2 },
                        authorsIds = new List<int> { 1 },
                        sourcesIds = new List<int> { 1 },
                        date = DateTime.UtcNow.Date
                    }
                };
            }

            var query = context.Articles
                .Include(a => a.Author)
                .Include(a => a.Category)
                .Include(a => a.Source)
                .AsQueryable();

            bool hasFilter =
                id.HasValue ||
                !string.IsNullOrWhiteSpace(title) ||
                (categoriesIds != null && categoriesIds.Any()) ||
                (authorsIds != null && authorsIds.Any()) ||
                (sourcesIds != null && sourcesIds.Any()) ||
                date.HasValue;

            if (hasFilter)
            {
                query = query.Where(a =>
                    (id.HasValue && a.Id == id.Value) ||
                    (!string.IsNullOrWhiteSpace(title) && a.Title.ToLower().Contains(title.Trim().ToLower())) ||
                    (categoriesIds != null && categoriesIds.Any() && categoriesIds.Contains(a.CategoryId)) ||
                    (authorsIds != null && authorsIds.Any() && authorsIds.Contains(a.AuthorId)) ||
                    (sourcesIds != null && sourcesIds.Any() && a.SourceId.HasValue && sourcesIds.Contains(a.SourceId.Value)) ||
                    (date.HasValue && a.CreateDate.Date == date.Value.Date)
                );
            }

            var total = await query.CountAsync();

            var articlesList = await query
                .OrderByDescending(a => a.UpDate)
                .Skip((currentPage - 1) * pageSize)
                .Take(pageSize)
                .ToListAsync();

            var checkSource = new CheckSource(context);
            var articlesWithSource = new List<object>();

            foreach (var article in articlesList)
            {
                var sourceUrl = await checkSource.CheckSources(article.Id);

                articlesWithSource.Add(new
                {
                    article.Id,
                    article.Title,
                    article.Summary,
                    article.CreateDate,
                    article.Imagem,
                    Author = article.Author?.Name,
                    Category = article.Category?.Name,
                    Source = article.Source?.Name,
                    SourceUrl = sourceUrl,
                    Text = withText ? article.Text : null
                });
            }

            return new
            {
                page = currentPage,
                pageSize = pageSize,
                total,
                totalPages = (int)Math.Ceiling(total / (double)pageSize),
                articles = articlesWithSource
            };
        }
    }
}
