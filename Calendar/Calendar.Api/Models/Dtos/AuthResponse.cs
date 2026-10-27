namespace Calendar.Api.Models.Dtos
{
    public record AuthResponse(string Token, DateTime ExpiresAt, UserDto User);
}
