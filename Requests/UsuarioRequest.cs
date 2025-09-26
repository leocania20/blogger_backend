namespace blogger_backend.Models;

public record UsuarioRequest(
    string Nome,
    string Email,
    string Senha,
    string Role
);
