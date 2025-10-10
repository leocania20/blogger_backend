namespace blogger_backend.Models
{
    public class CustomizedResearchModel
    {
        public int Id { get; set; }

     
        public int UserId { get; set; }
        public UserModel User { get; set; } = null!;

        
        public int? CategoryId { get; set; }
        public CategoryModel? Category { get; set; }

        public int? AuthorId { get; set; }
        public AuthorModel? Author { get; set; }

        public int? SourceId { get; set; }
        public SourceModel? Source { get; set; }

        public DateTime CreateDate { get; set; } = DateTime.UtcNow;
    }
}
