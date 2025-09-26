namespace blogger_backend.Models;

public record AutorRequest(
    string Nome,
    string Bio,
    string Email,
    int UsuarioId,
    bool Ativo
);
