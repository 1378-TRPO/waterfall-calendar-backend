using System.Security.Claims;

namespace Calendar.Api.Services
{
    public static class ClaimsPrincipalExtensions
    {
        public static int GetUserId(this ClaimsPrincipal principal)
        {
            var value = principal.FindFirstValue("sub");
            return int.TryParse(value, out var id)
                ? id
                : throw new InvalidOperationException("В токене нет корректного claim 'sub'.");
        }
    }
}
