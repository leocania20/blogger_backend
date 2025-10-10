namespace blogger_backend.Models;

public record CategoryRequest(
    string Name,
    string Description,
    string Tag,
    bool Active
);
