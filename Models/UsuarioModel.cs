namespace blogger_backend.Models
{
    public class UsuarioModel
    {
        public int Id { get; set; }
        public string Nome { get; set; } = null!;
        public string Email { get; set; } = null!;
        public string SenhaHash { get; set; } = null!;
        public string Role { get; set; } = "Leitor";
        public DateTime DataCadastro { get; set; } = DateTime.UtcNow;
        public DateTime? DataUltimoLogin { get; set; }
        public bool Ativo { get; set; } = true;

        public ICollection<ComentarioModel> Comentarios { get; set; } = new List<ComentarioModel>();
        public ICollection<NotificacaoModel> Notificacoes { get; set; } = new List<NotificacaoModel>();
    }
}
