namespace blogger_backend.Models
{
    public class CategoriaModel
    {
        public int Id { get; set; }
        public string Nome { get; set; } = null!;
        public string Descricao { get; set; } = null!;
        public string Slug { get; set; } = null!;
        public bool Ativo { get; set; } = true;

        public ICollection<ArtigoModel> Artigos { get; set; } = new List<ArtigoModel>();
 
    }
}