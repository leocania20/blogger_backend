namespace blogger_backend.Models
{
    public class NotificacaoModel
    {
        public int Id { get; set; }
        public string Titulo { get; set; } = null!;
        public string Mensagem { get; set; } = null!;
        public string Tipo { get; set; } = "Geral";
        public bool Lida { get; set; } = false;
        public DateTime DataCriacao { get; set; } = DateTime.UtcNow;
        public bool Ativo { get; set; } = true;

        public int UsuarioId { get; set; }
        public UsuarioModel Usuario { get; set; } = null!;

        public int? ArtigoId { get; set; }
        public ArtigoModel? Artigo { get; set; }
    }
}
