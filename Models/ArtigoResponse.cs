namespace blogger_backend.Models;

public record ArtigoResponse(

    string Titulo,
    string Slug,
    string Conteudo,
    string Resumo,
    string? Imagem,
    string Autor,
    DateTime DataCriacao
);
