using System.ComponentModel.DataAnnotations;

namespace blogger_backend.Models
{
    public class CommentRequest
    {
        [Required(ErrorMessage = "O texto do comentário é obrigatório.")]
        public string Text { get; set; } = null!;

        [Required(ErrorMessage = "O status é obrigatório.")]
        public string Status { get; set; } = null!;

        [Required(ErrorMessage = "O campo 'UserId' é obrigatório.")]
        public int UserId { get; set; }

        [Required(ErrorMessage = "O campo 'ArticleId' é obrigatório.")]
        public int ArticleId { get; set; }

        public bool Active { get; set; } = true;
    }
}
