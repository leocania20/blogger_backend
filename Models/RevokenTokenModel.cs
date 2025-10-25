namespace blogger_backend.Models
{
    public class RevokedTokenModel
    {
        public int Id { get; set; }
        public string Jti { get; set; } = null!;
        public DateTime RevokedAt { get; set; } = DateTime.UtcNow;
    }
}
