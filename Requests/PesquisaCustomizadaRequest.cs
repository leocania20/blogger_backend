namespace blogger_backend.Models
{
    public record PesquisaCustomizadaRequest(
        int UsuarioId,        // obrigatório
        int? CategoriaId,     // opcional
        int? AutorId,         // opcional
        int? FonteId          // opcional
    );
}
