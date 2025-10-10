namespace blogger_backend.Models
{
    public class UserModel
    {
        public int Id { get; set; }
        public string Name { get; set; } = null!;
        public string Email { get; set; } = null!;
        public string Password { get; set; } = null!;
        public string Role { get; set; } = "Leitor";
        public DateTime CreateDate { get; set; } = DateTime.UtcNow;
        public DateTime? LastLoginDate { get; set; }
        public bool Active { get; set; } = true;

        public ICollection<CommentModel> Comments { get; set; } = new List<CommentModel>();
        public ICollection<NotificationModel> Notification { get; set; } = new List<NotificationModel>();
        public ICollection<CustomizedResearchModel> CustomizedResearchModel { get; set; } = new List<CustomizedResearchModel>();

    }
}
