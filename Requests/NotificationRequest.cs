namespace blogger_backend.Models;

public record NotificationRequest(
    string Title,
    string Message,
    string Type,
    int UserId,
    int? ArticleId,
    bool Readed
);
