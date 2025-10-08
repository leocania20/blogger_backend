using System;
using System.Threading.Tasks;
using blogger_backend.Data;
using blogger_backend.Models;
using Microsoft.EntityFrameworkCore;

namespace blogger_backend.Utils
{
    public class VerificationFonte
    {
        private readonly AppDbContext _context;

        public VerificationFonte(AppDbContext context)
        {
            _context = context;
        }

        public async Task<string?> ObterUrlFontePorArtigoAsync(int artigoId)
        {
            var artigo = await _context.Artigos
                .Include(a => a.Fonte)
                .FirstOrDefaultAsync(a => a.Id == artigoId);
            if (artigo == null)
                return null; 
            if (artigo.FonteId == null || artigo.Fonte == null)
                return null;
            if (artigo.Fonte.Tipo.ToLower() == "interna")
            {
                if (!string.IsNullOrWhiteSpace(artigo.Fonte.URL))
                    return artigo.Fonte.URL;
                return null;
            }
            if (artigo.Fonte.Tipo.ToLower() == "externa")
                return artigo.Fonte.URL;
            return null;
        }
    }
}
