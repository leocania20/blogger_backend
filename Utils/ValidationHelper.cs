using System.ComponentModel.DataAnnotations;

namespace blogger_backend.Utils
{
    public static class ValidationHelper
    {
        public static List<string> ValidateModel<T>(T model)
        {
            var results = new List<ValidationResult>();
            var context = new ValidationContext(model!);
            Validator.TryValidateObject(model!, context, results, true);

            var errors = new List<string>();
            foreach (var validationResult in results)
            {
                errors.Add(validationResult.ErrorMessage ?? "Erro de validação.");
            }

            return errors;
        }
    }
}
