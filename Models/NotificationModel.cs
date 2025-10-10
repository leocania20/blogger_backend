namespace blogger_backend.Models
{
    public class NotificationModel
    {
        public int Id { get; set; }
        public string Title { get; set; } = null!;
        public string Message { get; set; } = null!;
        public string Type { get; set; } = "Geral";
        public bool Readed { get; set; } = false;
        public DateTime CreateDate { get; set; } = DateTime.UtcNow;
        public bool Active { get; set; } = true;

        public int UserId { get; set; }
        public UserModel User { get; set; } = null!;

        public int? ArticleId { get; set; }
        public ArticlesModel? Article { get; set; }
    }
}
