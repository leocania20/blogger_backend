namespace blogger_backend.Models;

public record CategoriaRequest(
    string Nome,
    string Descricao,
    string Slug,
    bool Ativo
);
