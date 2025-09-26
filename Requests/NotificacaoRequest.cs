namespace blogger_backend.Models;

public record NotificacaoRequest(
    string Titulo,
    string Mensagem,
    string Tipo,
    int UsuarioId,
    int? ArtigoId,
    bool Lida
);
