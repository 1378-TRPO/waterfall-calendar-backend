using System.ComponentModel.DataAnnotations;

namespace Calendar.Api.Services
{
    public class JwtOptions
    {
        public const string SectionName = "Jwt";

        [Required, MinLength(32)]
        public string Key { get; set; } = string.Empty;

        [Required]
        public string Issuer { get; set; } = string.Empty;

        [Required]
        public string Audience { get; set; } = string.Empty;

        [Range(1, 24 * 30)]
        public int ExpiresInHours { get; set; } = 8;
    }
}
