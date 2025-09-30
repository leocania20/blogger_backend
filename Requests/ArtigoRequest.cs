namespace blogger_backend.Models;

public record ArtigoRequest(
   
    string Titulo,
    string Slug,
    string Conteudo,
    string Resumo,
    bool IsPublicado,
    int CategoriaId,
    int AutorId,
    int? FonteId,
    string? Imagem

);




