using System.ComponentModel.DataAnnotations;

namespace blogger_backend.Models;

public class NewsletterRequest {

    [Required(ErrorMessage = "O e-mail é obrigatório.")]
    [EmailAddress(ErrorMessage = "O e-mail fornecido não é válido.")]
    public string Email { get; set; } = null!;
    bool Active;
}
public class NewsletterSendRequest
    {
        public string Subject { get; set; } = null!;
        public string HtmlBody { get; set; } = null!;
        public string PlainText { get; set; } = string.Empty;
        public bool OnlyActive { get; set; } = true;
    }