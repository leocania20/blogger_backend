namespace blogger_backend.Models
{
    public class FonteModel
    {
         public int Id { get; set; }
        public string Nome { get; set; } = null!;
        public string URL { get; set; } = null!;
        public string Tipo { get; set; } = null!;
        public bool Ativo { get; set; } = true;

        public ICollection<ArtigoModel> Artigos { get; set; } = new List<ArtigoModel>();

    }
}