using System.ComponentModel.DataAnnotations;

namespace Calendar.Api.Models.Dtos
{
    public class LoginRequest
    {
        [Required, MaxLength(256)]
        public string Email { get; set; } = string.Empty;

        [Required, MaxLength(72)]
        public string Password { get; set; } = string.Empty;
    }
}
