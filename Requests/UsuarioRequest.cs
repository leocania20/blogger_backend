namespace blogger_backend.Models;

public record UsuarioRequest(
    string Nome,
    string Email,
    string Senha,
    string Role
);
public record UsuarioRegisterRequest(
        string Nome,
        string Email,
        string Senha
    );

    public record UsuarioLoginRequest(
        string Email,
        string Senha
    );