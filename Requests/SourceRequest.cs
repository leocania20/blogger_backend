using System.ComponentModel.DataAnnotations;

namespace blogger_backend.Requests
{
    public class SourceRequest
    {
        [Required(ErrorMessage = "O campo Nome é obrigatório.")]
        [StringLength(100, ErrorMessage = "O nome deve ter no máximo 100 caracteres.")]
        public string Name { get; set; } = string.Empty;

        [Required(ErrorMessage = "O campo URL é obrigatório.")]
        [Url(ErrorMessage = "A URL informada não é válida.")]
        public string URL { get; set; } = string.Empty;

        [Required(ErrorMessage = "O campo Tipo é obrigatório.")]
        [StringLength(50)]
        public string Type { get; set; } = string.Empty;
    }
}
