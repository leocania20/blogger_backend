namespace blogger_backend.Models
{
    public class PesquisaCustomizadaModel
    {
        public int Id { get; set; }

     
        public int UsuarioId { get; set; }
        public UsuarioModel Usuario { get; set; } = null!;

        
        public int? CategoriaId { get; set; }
        public CategoriaModel? Categoria { get; set; }

        public int? AutorId { get; set; }
        public AutorModel? Autor { get; set; }

        public int? FonteId { get; set; }
        public FonteModel? Fonte { get; set; }

        public DateTime DataCriacao { get; set; } = DateTime.UtcNow;
    }
}
