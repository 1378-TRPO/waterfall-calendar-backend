using System.ComponentModel.DataAnnotations;

namespace Calendar.Api.Models.Dtos
{
    public class RegisterRequest
    {
        [Required, EmailAddress, MaxLength(256)]
        public string Email { get; set; } = string.Empty;

        // BCrypt учитывает только первые 72 байта пароля.
        [Required, MinLength(6), MaxLength(72)]
        public string Password { get; set; } = string.Empty;

        [Required, MaxLength(100)]
        public string DisplayName { get; set; } = string.Empty;
    }
}
