namespace blogger_backend.Models
{
    public class AccessLogModel
    {
        public int Id { get; set; }
        public string Route { get; set; } = string.Empty;
        public string Method { get; set; } = string.Empty;
        public string IpAddress { get; set; } = string.Empty;
        public string? UserAgent { get; set; }
        public string? UserId { get; set; }
        public string? UserName { get; set; }
        public double DurationMs { get; set; }  
        public DateTime AccessDate { get; set; } = DateTime.UtcNow;
    }
}
