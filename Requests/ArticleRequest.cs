namespace blogger_backend.Models;

public record ArticleRequest(
   
    string Title,
    string Tag,
    string Text,
    string Summary,
    bool IsPublished,
    int CategoryId,
    int AuthorId,
    int? SourceId,
    string? Imagem

);




