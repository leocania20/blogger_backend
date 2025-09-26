namespace blogger_backend.Models
{
    public class ComentarioModel
    {
        public int Id { get; set; }
        public string Conteudo { get; set; } = null!;
        public string Status { get; set; } = "Pendente";
        public DateTime DataComentario { get; set; } = DateTime.UtcNow;

        public int UsuarioId { get; set; }
        public UsuarioModel Usuario { get; set; } = null!;

        public bool Ativo { get; set; } = true;

        public DateTime DataCriacao { get; set; }

        public int ArtigoId { get; set; }
        public ArtigoModel Artigo { get; set; } = null!;
    }
}

    
