namespace Calendar.Api.Services
{
    /// <summary>Ошибка, которая превращается в ProblemDetails с указанным кодом.</summary>
    public class ApiException : Exception
    {
        public int StatusCode { get; }
        public IDictionary<string, string[]>? Errors { get; }

        public ApiException(int statusCode, string title, IDictionary<string, string[]>? errors = null)
            : base(title)
        {
            StatusCode = statusCode;
            Errors = errors;
        }

        public static ApiException NotFound(string title) => new(StatusCodes.Status404NotFound, title);
        public static ApiException Forbidden(string title) => new(StatusCodes.Status403Forbidden, title);
        public static ApiException Conflict(string title) => new(StatusCodes.Status409Conflict, title);
        public static ApiException Validation(IDictionary<string, string[]> errors) =>
            new(StatusCodes.Status400BadRequest, "Ошибка валидации", errors);
    }
}
