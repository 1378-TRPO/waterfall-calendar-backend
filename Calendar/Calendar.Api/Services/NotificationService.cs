using Calendar.Api.Data;
using Calendar.Api.Models;
using Calendar.Api.Models.Dtos;
using Microsoft.EntityFrameworkCore;

namespace Calendar.Api.Services
{
    public interface INotificationService
    {
        Task<List<NotificationDto>> GetAsync(int userId);
        Task<UnreadCountDto> GetUnreadCountAsync(int userId);
        Task<NotificationDto> MarkReadAsync(int userId, int notificationId);

        // Методы Stage* только добавляют уведомления в контекст: они сохраняются вместе с самой записью
        // одним SaveChanges, поэтому уведомление не бывает без события и наоборот.
        void StageInvited(CalendarEvent calendarEvent, IEnumerable<int> recipientIds);
        void StageChanged(CalendarEvent calendarEvent, IEnumerable<int> recipientIds);
        void StageCancelled(CalendarEvent calendarEvent, IEnumerable<int> recipientIds);
        void StagePinged(CalendarEvent calendarEvent, int recipientId);
    }

    public class NotificationService : INotificationService
    {
        private const int ListLimit = 50;
        private const int MaxMessageLength = 300;

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

        public void StageInvited(CalendarEvent calendarEvent, IEnumerable<int> recipientIds) =>
            Stage(
                calendarEvent,
                recipientIds,
                NotificationType.Invited,
                $"Вас приглашают на встречу «{calendarEvent.Title}» (организатор: {calendarEvent.Organizer.DisplayName})");

        public void StageChanged(CalendarEvent calendarEvent, IEnumerable<int> recipientIds) =>
            Stage(calendarEvent, recipientIds, NotificationType.Changed, $"Встреча «{calendarEvent.Title}» изменена");

        public void StageCancelled(CalendarEvent calendarEvent, IEnumerable<int> recipientIds) =>
            Stage(calendarEvent, recipientIds, NotificationType.Cancelled, $"Встреча «{calendarEvent.Title}» отменена");

        public void StagePinged(CalendarEvent calendarEvent, int recipientId) =>
            Stage(
                calendarEvent,
                [recipientId],
                NotificationType.Pinged,
                $"{calendarEvent.Organizer.DisplayName} напоминает о встрече «{calendarEvent.Title}»");

        private void Stage(
            CalendarEvent calendarEvent, IEnumerable<int> recipientIds, NotificationType type, string message)
        {
            var text = message.Length <= MaxMessageLength ? message : message[..(MaxMessageLength - 1)] + "…";
            var now = DateTime.UtcNow;

            foreach (var recipientId in recipientIds)
            {
                _db.Notifications.Add(new Notification
                {
                    UserId = recipientId,
                    Event = calendarEvent,
                    Type = type,
                    Message = text,
                    IsRead = false,
                    CreatedAt = now
                });
            }
        }

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
