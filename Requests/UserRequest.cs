namespace blogger_backend.Models;
using System.ComponentModel.DataAnnotations;
 public class UserRequest
    {
        [Required(ErrorMessage = "O nome é obrigatório.")]
        [MinLength(3, ErrorMessage = "O nome deve ter pelo menos 3 caracteres.")]
        [RegularExpression(@"^[A-Za-zÀ-ÿ\s]+$", ErrorMessage = "O nome só pode conter letras e espaços.")]
        public string Name { get; set; } = null!;

        [Required(ErrorMessage = "O e-mail é obrigatório.")]
        [EmailAddress(ErrorMessage = "O e-mail fornecido não é válido.")]
        public string Email { get; set; } = null!;

        [MinLength(6, ErrorMessage = "A senha deve ter pelo menos 6 caracteres.")]
        public string? Password { get; set; }

        [Required(ErrorMessage = "O papel é obrigatório.")]
        public string Role { get; set; } = "Leitor";
    }
public record UserRegisterRequest(
        string Name,
        string Email,
        string Password
    );

public record UserLoginRequest(
       string Email,
       string Password
);
public class RefreshRequest
    {
        public string RefreshToken { get; set; } = null!;
    }