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
            var query = context.Articles
                .Include(a => a.Author)
                .Include(a => a.Category)
                .Include(a => a.Source)
                .Where(a => a.IsPublished)
                .AsQueryable();

            if (id.HasValue)
                query = query.Where(a => a.Id == id.Value);

            if (!string.IsNullOrWhiteSpace(title))
                query = query.Where(a => a.Title.ToLower().Contains(title.ToLower()));

            if (categoriesIds != null && categoriesIds.Any())
                query = query.Where(a => categoriesIds.Contains(a.CategoryId));

            if (authorsIds != null && authorsIds.Any())
                query = query.Where(a => authorsIds.Contains(a.AuthorId));

            if (sourcesIds != null && sourcesIds.Any())
                query = query.Where(a => a.SourceId.HasValue && sourcesIds.Contains(a.SourceId.Value));

            if (date.HasValue)
                query = query.Where(a => a.CreateDate.Date == date.Value.Date);

            var total = await query.CountAsync();

            var articlesQuery = query
                .OrderByDescending(a => a.UpDate)
                .Skip((currentPage - 1) * pageSize)
                .Take(pageSize)
                .ToListAsync();

            var articlesList = await articlesQuery;

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
                    Author = article.Author.Name,
                    SourceUrl = sourceUrl,
                    Text = withText ? article.Text : null
                });
            }

            return new
            {
                Page = currentPage,
                PageSize = pageSize,
                Total = total,
                TotalPages = (int)Math.Ceiling(total / (double)pageSize),
                Articles = articlesWithSource
            };
        }
    }
}
