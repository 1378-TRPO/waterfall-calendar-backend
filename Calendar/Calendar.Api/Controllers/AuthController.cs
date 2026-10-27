using Calendar.Api.Models.Dtos;
using Calendar.Api.Services;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace Calendar.Api.Controllers
{
    [ApiController]
    [Route("api/auth")]
    public class AuthController : ControllerBase
    {
        private readonly IAuthService _auth;
        private readonly IUserService _users;

        public AuthController(IAuthService auth, IUserService users)
        {
            _auth = auth;
            _users = users;
        }

        [HttpPost("register")]
        [AllowAnonymous]
        public async Task<ActionResult<AuthResponse>> Register(RegisterRequest request)
        {
            var result = await _auth.RegisterAsync(request);
            return result is null
                ? Problem(statusCode: StatusCodes.Status409Conflict, title: "Email уже занят")
                : Ok(result);
        }

        [HttpPost("login")]
        [AllowAnonymous]
        public async Task<ActionResult<AuthResponse>> Login(LoginRequest request)
        {
            var result = await _auth.LoginAsync(request);
            return result is null
                ? Problem(statusCode: StatusCodes.Status401Unauthorized, title: "Неверный email или пароль")
                : Ok(result);
        }

        [HttpGet("me")]
        [Authorize]
        public async Task<ActionResult<UserDto>> Me()
        {
            var user = await _users.GetByIdAsync(User.GetUserId());
            return user is null ? Unauthorized() : Ok(user);
        }
    }
}
