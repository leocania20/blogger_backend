namespace blogger_backend.Models
{
    public class CommentModel
    {
        public int Id { get; set; }
        public string Text { get; set; } = null!;
        public string Status { get; set; } = "Pendente";
        public DateTime CommentData { get; set; } = DateTime.UtcNow;

        public int? UserId { get; set; }
        public UserModel User { get; set; } = null!;

        public bool Active { get; set; } = true;

        public DateTime CreateDate { get; set; }

        public int ArticleId { get; set; }
        public ArticlesModel Article { get; set; } = null!;
    }
}

    
