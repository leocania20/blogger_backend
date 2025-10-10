namespace blogger_backend.Models;

public record UserRequest(
    string Name,
    string Email,
    string Password,
    string Role
);
public record UserRegisterRequest(
        string Name,
        string Email,
        string Password
    );

    public record UserLoginRequest(
        string Email,
        string Password
    );