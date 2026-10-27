using Calendar.Api.Data;
using Calendar.Api.Models.Dtos;
using Microsoft.EntityFrameworkCore;

namespace Calendar.Api.Services
{
    public interface IUserService
    {
        Task<List<UserDto>> GetAllAsync();
        Task<UserDto?> GetByIdAsync(int id);
    }

    public class UserService : IUserService
    {
        private readonly AppDbContext _db;

        public UserService(AppDbContext db)
        {
            _db = db;
        }

        public Task<List<UserDto>> GetAllAsync() =>
            _db.Users
                .AsNoTracking()
                .OrderBy(u => u.DisplayName)
                .Select(u => new UserDto(u.Id, u.Email, u.DisplayName))
                .ToListAsync();

        public Task<UserDto?> GetByIdAsync(int id) =>
            _db.Users
                .AsNoTracking()
                .Where(u => u.Id == id)
                .Select(u => new UserDto(u.Id, u.Email, u.DisplayName))
                .FirstOrDefaultAsync();
    }
}
