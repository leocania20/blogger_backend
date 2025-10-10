namespace blogger_backend.Models;

public record AuthorRequest(
    string Name,
    string Bio,
    string Email,
    int UserId,
    bool Active
);
