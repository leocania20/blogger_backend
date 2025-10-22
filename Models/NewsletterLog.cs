namespace blogger_backend.Models
{
    public class NewsletterLog
    {
        public int Id { get; set; }
        public int? NewsletterId { get; set; }        
        public string ToEmail { get; set; } = null!;
        public string Subject { get; set; } = null!;
        public bool Success { get; set; }
        public string? ResponseBody { get; set; }   
        public int? StatusCode { get; set; }          
        public DateTime CreateDate { get; set; } = DateTime.UtcNow;
    }
}
