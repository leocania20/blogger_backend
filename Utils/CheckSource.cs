using blogger_backend.Data;
using Microsoft.EntityFrameworkCore;

namespace blogger_backend.Utils
{
    public class CheckSource
    {
        private readonly AppDbContext _context;

        public CheckSource(AppDbContext context)
        {
            _context = context;
        }

        public async Task<string?> CheckSources (int articleId)
        {
            var article = await _context.Articles
                .Include(a => a.Source)
                .FirstOrDefaultAsync(a => a.Id == articleId);
            if (article == null)
                return null; 
            if (article.SourceId == null || article.Source == null)
                return null;
            if (article.Source.Type.ToLower() == "interna")
            {
                if (!string.IsNullOrWhiteSpace(article.Source.URL))
                    return article.Source.URL;
                return null;
            }
            if (article.Source.Type.ToLower() == "externa")
                return article.Source.URL;
            return null;
        }
    }
}
