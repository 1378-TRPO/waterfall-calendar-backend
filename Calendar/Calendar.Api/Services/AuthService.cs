using Calendar.Api.Data;
using Calendar.Api.Models;
using Calendar.Api.Models.Dtos;
using Microsoft.EntityFrameworkCore;

namespace Calendar.Api.Services
{
    public interface IAuthService
    {
        /// <summary>Возвращает null, если email уже занят.</summary>
        Task<AuthResponse?> RegisterAsync(RegisterRequest request);

        /// <summary>Возвращает null, если email или пароль неверны.</summary>
        Task<AuthResponse?> LoginAsync(LoginRequest request);
    }

    public class AuthService : IAuthService
    {
        private readonly AppDbContext _db;
        private readonly ITokenService _tokens;
        private readonly ILogger<AuthService> _logger;

        public AuthService(AppDbContext db, ITokenService tokens, ILogger<AuthService> logger)
        {
            _db = db;
            _tokens = tokens;
            _logger = logger;
        }

        public async Task<AuthResponse?> RegisterAsync(RegisterRequest request)
        {
            var email = NormalizeEmail(request.Email);

            if (await _db.Users.AnyAsync(u => u.Email == email))
            {
                _logger.LogInformation("Регистрация отклонена: email {Email} уже занят", email);
                return null;
            }

            var user = new User
            {
                Email = email,
                PasswordHash = BCrypt.Net.BCrypt.HashPassword(request.Password),
                DisplayName = request.DisplayName.Trim(),
                CreatedAt = DateTime.UtcNow
            };
            _db.Users.Add(user);

            try
            {
                await _db.SaveChangesAsync();
            }
            catch (DbUpdateException)
            {
                // Параллельная регистрация того же email: сработал unique-индекс.
                _logger.LogInformation("Регистрация отклонена: email {Email} уже занят", email);
                return null;
            }

            _logger.LogInformation("Зарегистрирован пользователь {UserId}", user.Id);
            return CreateResponse(user);
        }

        public async Task<AuthResponse?> LoginAsync(LoginRequest request)
        {
            var email = NormalizeEmail(request.Email);
            var user = await _db.Users.FirstOrDefaultAsync(u => u.Email == email);

            if (user is null || !BCrypt.Net.BCrypt.Verify(request.Password, user.PasswordHash))
            {
                _logger.LogInformation("Неудачный вход для {Email}", email);
                return null;
            }

            return CreateResponse(user);
        }

        private AuthResponse CreateResponse(User user)
        {
            var (token, expiresAt) = _tokens.CreateToken(user);
            return new AuthResponse(token, expiresAt, new UserDto(user.Id, user.Email, user.DisplayName));
        }

        private static string NormalizeEmail(string email) => email.Trim().ToLowerInvariant();
    }
}
