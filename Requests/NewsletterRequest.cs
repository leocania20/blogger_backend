namespace blogger_backend.Models;

public record NewsletterRequest(
    string Email,
    bool Ativo
);
