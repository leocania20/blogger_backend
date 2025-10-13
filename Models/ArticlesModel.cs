namespace blogger_backend.Models
{
    public class ArticlesModel
    {
        public int Id { get; set; }
        public string Title { get; set; } = null!;
        public string? Tag { get; set; } = null!;
        public string Text { get; set; } = null!;
        public string Summary { get; set; } = null!;
        public DateTime CreateDate { get; set; } = DateTime.UtcNow;
        public DateTime? UpDate { get; set; }
        public DateTime? PublishedDate { get; set; }
        public bool IsPublished { get; set; } = false;

        public string? Imagem { get; set; } 

        public int CategoryId { get; set; }
        public CategoryModel Category { get; set; } = null!;

        public int AuthorId { get; set; }
        public AuthorModel Author { get; set; } = null!;

        public int? SourceId { get; set; }
        public SourceModel? Source { get; set; }

        public ICollection<CommentModel> Comment { get; set; } = new List<CommentModel>();
        public ICollection<NotificationModel> Notification { get; set; } = new List<NotificationModel>();
   
    }
}