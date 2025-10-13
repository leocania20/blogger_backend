
using System.ComponentModel.DataAnnotations;
namespace blogger_backend.Models
{

    public class AuthorRequest
    {
        [Required(ErrorMessage = "O nome é obrigatório.")]
        public string Name { get; set; } = null!;

        [Required(ErrorMessage = "A biografia é obrigatória.")]
        public string Bio { get; set; } = null!;

        [Required(ErrorMessage = "O e-mail é obrigatório.")]
        [EmailAddress(ErrorMessage = "O e-mail informado não é válido.")]
        public string Email { get; set; } = null!;

        [Required(ErrorMessage = "O campo 'UserId' é obrigatório.")]
        public int UserId { get; set; }
    }
}
