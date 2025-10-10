namespace blogger_backend.Models;

public record ArticlesResponse(

    string Title,
    string Slug,
    string Text,
    string Resummary,
    string? Imagem,
    string Author,
    DateTime CreateDate
);
