namespace blogger_backend.Models
{
    public class NewsletterModel
    {
        public int Id { get; set; }
        public string Email { get; set; } = null!;
        public DateTime CreateDate { get; set; } = DateTime.UtcNow;
        public bool Active { get; set; } = true;
    }
}
