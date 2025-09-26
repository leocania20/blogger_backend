namespace blogger_backend.Models;

public record ComentarioRequest(
    string Conteudo,
    int UsuarioId,
    int ArtigoId,
    string Status,
    bool Ativo = true,
    DateTime? DataCriacao = null
);
