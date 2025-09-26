namespace blogger_backend.Models
{
    public class AutorModel
    {
        public int Id { get; set; }
        public string Nome { get; set; } = null!;
        public string Bio { get; set; } = null!;
        public string Email { get; set; } = null!;
        public int? UsuarioId { get; set; }
        public UsuarioModel? Usuario { get; set; }

        public bool Ativo { get; set; } = true;

        public ICollection<ArtigoModel> Artigos { get; set; } = new List<ArtigoModel>();
    }
}