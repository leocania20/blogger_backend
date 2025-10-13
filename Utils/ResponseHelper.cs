namespace blogger_backend.Utils
{
    public static class ResponseHelper
    {
        public static IResult Ok(object data) =>
            Results.Ok(new { Success = true, Data = data });

        public static IResult Created(string path, object data) =>
            Results.Created(path, new { Success = true, Data = data });

        public static IResult BadRequest(string message) =>
            Results.BadRequest(new { Success = false, Message = message });

        public static IResult Conflict(string message) =>
            Results.Conflict(new { Success = false, Message = message });

        public static IResult NotFound(string message = "Recurso não encontrado.") =>
            Results.NotFound(new { Success = false, Message = message });

        public static IResult Unauthorized(string message = "Não autorizado.") =>
            Results.Unauthorized();

        public static IResult ServerError(string message = "Erro interno no servidor.") =>
            Results.Problem(message);
    }
}
