namespace blogger_backend.Utils
{
    public static class ResponseHelper
    {
        public static IResult Ok(object data, string? message = null)
            => Results.Ok(new { success = true, message = message ?? "Operação realizada com sucesso.", data });

        public static IResult Created(string path, object data, string? message = null)
            => Results.Created(path, new { success = true, message = message ?? "Recurso criado com sucesso.", data });

        public static IResult BadRequest(string message, object? errors = null, object? example = null)
            => Results.BadRequest(new { success = false, message, errors, example });

        public static IResult Conflict(string message, object? example = null)
            => Results.Conflict(new { success = false, message, example });

        public static IResult NotFound(string message = "Recurso não encontrado.", object? example = null)
            => Results.NotFound(new { success = false, message, example });

        public static IResult ServerError(string message = "Erro interno no servidor.")
            => Results.Problem(message);
    }
}
