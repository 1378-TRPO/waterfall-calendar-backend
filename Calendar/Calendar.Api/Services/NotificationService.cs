using Calendar.Api.Data;
using Calendar.Api.Models.Dtos;
using Microsoft.EntityFrameworkCore;

namespace Calendar.Api.Services
{
    public interface INotificationService
    {
        Task<List<NotificationDto>> GetAsync(int userId);
        Task<UnreadCountDto> GetUnreadCountAsync(int userId);
        Task<NotificationDto> MarkReadAsync(int userId, int notificationId);
    }

    public class NotificationService : INotificationService
    {
        private const int ListLimit = 50;

        private readonly AppDbContext _db;
        private readonly ILogger<NotificationService> _logger;

        public NotificationService(AppDbContext db, ILogger<NotificationService> logger)
        {
            _db = db;
            _logger = logger;
        }

        public Task<List<NotificationDto>> GetAsync(int userId) =>
            _db.Notifications
                .AsNoTracking()
                .Where(n => n.UserId == userId)
                .OrderByDescending(n => n.CreatedAt)
                .ThenByDescending(n => n.Id)
                .Take(ListLimit)
                .Select(n => new NotificationDto(n.Id, n.Type, n.Message, n.IsRead, n.CreatedAt, n.EventId))
                .ToListAsync();

        public async Task<UnreadCountDto> GetUnreadCountAsync(int userId) =>
            new(await _db.Notifications.CountAsync(n => n.UserId == userId && !n.IsRead));

        public async Task<NotificationDto> MarkReadAsync(int userId, int notificationId)
        {
            // Чужое уведомление неотличимо от несуществующего: так не раскрываем, что оно есть.
            var notification = await _db.Notifications
                .FirstOrDefaultAsync(n => n.Id == notificationId && n.UserId == userId)
                ?? throw ApiException.NotFound("Уведомление не найдено.");

            if (!notification.IsRead)
            {
                notification.IsRead = true;
                await _db.SaveChangesAsync();
                _logger.LogInformation("Пользователь {UserId} прочитал уведомление {NotificationId}", userId, notificationId);
            }

            return new NotificationDto(
                notification.Id,
                notification.Type,
                notification.Message,
                notification.IsRead,
                notification.CreatedAt,
                notification.EventId);
        }
    }
}
