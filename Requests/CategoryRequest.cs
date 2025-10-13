namespace blogger_backend.Models;
using System.ComponentModel.DataAnnotations;


public record CategoryRequest
{

    [Required(ErrorMessage = "O nome da categoria é obrigatório.")]
    [StringLength(100, ErrorMessage = "O nome deve ter no máximo 100 caracteres.")]
    public string Name { get; set; } = null!;

    [Required(ErrorMessage = "A tag da categoria é obrigatória.")]
    [StringLength(50, ErrorMessage = "A tag deve ter no máximo 50 caracteres.")]
    public string Tag { get; set; } = null!;

    [StringLength(500, ErrorMessage = "A descrição deve ter no máximo 500 caracteres.")]
    public string? Description { get; set; }

    public bool Active { get; set; } = true;
}
