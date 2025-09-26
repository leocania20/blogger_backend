namespace blogger_backend.Models
{
    public class NewsletterModel
    {
        public int Id { get; set; }
        public string Email { get; set; } = null!;
        public DateTime DataCadastro { get; set; } = DateTime.UtcNow;
        public bool Ativo { get; set; } = true;
    }
}
