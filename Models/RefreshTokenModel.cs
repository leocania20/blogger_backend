namespace blogger_backend.Models
{
    public class RefreshTokenModel
    {
        public int Id { get; set; } 
        public int UserId { get; set; } 
        public string Token { get; set; } = null!;
        public DateTime Expiration { get; set; }
        public bool Revoked { get; set; } = false;
        public UserModel User { get; set; } = null!;
    }
}
