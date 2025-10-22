namespace blogger_backend.Models;
using System.ComponentModel.DataAnnotations;
public class ArticleRequest
    {
        [Required(ErrorMessage = "O título é obrigatório.")]
        public string Title { get; set; } = null!;

        [Required(ErrorMessage = "O resumo é obrigatório.")]
        public string Summary { get; set; } = null!;

        [Required(ErrorMessage = "O texto é obrigatório.")]
        public string Text { get; set; } = null!;

        public string? Tag { get; set; }

        [Required(ErrorMessage = "O campo 'CategoryId' é obrigatório.")]
        public int CategoryId { get; set; }

        [Required(ErrorMessage = "O campo 'AuthorId' é obrigatório.")]
        public int AuthorId { get; set; }

        [Required(ErrorMessage = "O campo 'SourceId' é obrigatório.")]
        public int SourceId { get; set; }

        public IFormFile? Imagem { get; set; } = null!;
        public bool IsPublished { get; set; } = false;
    }




