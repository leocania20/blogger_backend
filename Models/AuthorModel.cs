namespace blogger_backend.Models
{
    public class AuthorModel
    {
        public int Id { get; set; }
        public string Name { get; set; } = null!;
        public string Bio { get; set; } = null!;
        public string Email { get; set; } = null!;
        public int? UserId { get; set; }
        public UserModel? User { get; set; }

        public bool Active { get; set; } = true;

        public ICollection<ArticlesModel> Articles { get; set; } = new List<ArticlesModel>();
    }
}