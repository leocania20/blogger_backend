namespace blogger_backend.Models
{
    public class ArtigoModel
    {
        public int Id { get; set; }
        public string Titulo { get; set; } = null!;
        public string Slug { get; set; } = null!;
        public string Conteudo { get; set; } = null!;
        public string Resumo { get; set; } = null!;
        public DateTime DataCriacao { get; set; } = DateTime.UtcNow;
        public DateTime? DataAtualizacao { get; set; }
        public DateTime? DataPublicacao { get; set; }
        public bool IsPublicado { get; set; } = false;

        public string? Imagem { get; set; } 

        public int CategoriaId { get; set; }
        public CategoriaModel Categoria { get; set; } = null!;

        public int AutorId { get; set; }
        public AutorModel Autor { get; set; } = null!;

        public int? FonteId { get; set; }
        public FonteModel? Fonte { get; set; }

        public ICollection<ComentarioModel> Comentarios { get; set; } = new List<ComentarioModel>();
        public ICollection<NotificacaoModel> Notificacoes { get; set; } = new List<NotificacaoModel>();
   
    }
}