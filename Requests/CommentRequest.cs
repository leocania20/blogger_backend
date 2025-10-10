namespace blogger_backend.Models;

public record CommentRequest(
    string Text,
    int UserId,
    int ArticleId,
    string Status,
    bool Active = true,
    DateTime? Createdate = null
);
