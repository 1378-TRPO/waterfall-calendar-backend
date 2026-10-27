using Calendar.Api.Data;
using Calendar.Api.Models;
using Calendar.Api.Models.Dtos;
using Microsoft.EntityFrameworkCore;

namespace Calendar.Api.Services
{
    public interface IEventService
    {
        Task<EventDetailsDto> CreateAsync(int userId, CreateEventRequest request);
        Task<EventDetailsDto> GetAsync(int userId, int eventId);
        Task<List<EventSummaryDto>> GetRangeAsync(int userId, DateTimeOffset from, DateTimeOffset to);
        Task<List<EventSummaryDto>> GetUpcomingAsync(int userId, int limit);
    }

    public class EventService : IEventService
    {
        private readonly AppDbContext _db;
        private readonly ILogger<EventService> _logger;

        public EventService(AppDbContext db, ILogger<EventService> logger)
        {
            _db = db;
            _logger = logger;
        }

        public async Task<EventDetailsDto> CreateAsync(int userId, CreateEventRequest request)
        {
            var type = request.Type!.Value;
            var participantIds = await ValidateAsync(type, request, userId);

            var now = DateTime.UtcNow;
            var calendarEvent = new CalendarEvent
            {
                Type = type,
                Title = request.Title.Trim(),
                Description = NullIfBlank(request.Description),
                StartAt = request.StartAt!.Value.UtcDateTime,
                EndAt = request.EndAt!.Value.UtcDateTime,
                MeetingUrl = NullIfBlank(request.MeetingUrl),
                OrganizerId = userId,
                CreatedAt = now,
                UpdatedAt = now,
                Participants = participantIds
                    .Select(id => new EventParticipant { UserId = id, RsvpStatus = RsvpStatus.Pending })
                    .ToList()
            };

            _db.CalendarEvents.Add(calendarEvent);
            await _db.SaveChangesAsync();

            _logger.LogInformation("Пользователь {UserId} создал {Type} {EventId}", userId, type, calendarEvent.Id);
            return await GetAsync(userId, calendarEvent.Id);
        }

        public async Task<EventDetailsDto> GetAsync(int userId, int eventId)
        {
            var calendarEvent = await LoadAsync(eventId, track: false);
            EnsureCanView(calendarEvent, userId);
            return ToDetails(calendarEvent, userId);
        }

        public async Task<List<EventSummaryDto>> GetRangeAsync(int userId, DateTimeOffset from, DateTimeOffset to)
        {
            if (to <= from)
            {
                throw ApiException.Validation(new Dictionary<string, string[]>
                {
                    ["to"] = ["Конец периода должен быть позже начала."]
                });
            }

            var fromUtc = from.UtcDateTime;
            var toUtc = to.UtcDateTime;

            return await SummariesFor(userId)
                .Where(e => e.StartAt < toUtc && e.EndAt > fromUtc)
                .OrderBy(e => e.StartAt)
                .ThenBy(e => e.Id)
                .ToListAsync();
        }

        public async Task<List<EventSummaryDto>> GetUpcomingAsync(int userId, int limit)
        {
            var now = DateTime.UtcNow;

            return await SummariesFor(userId)
                .Where(e => e.StartAt > now && !e.IsCancelled && e.MyRsvpStatus != RsvpStatus.Declined)
                .OrderBy(e => e.StartAt)
                .ThenBy(e => e.Id)
                .Take(limit)
                .ToListAsync();
        }

        // Запись видна, если пользователь организатор или участник.
        private IQueryable<EventSummaryDto> SummariesFor(int userId) =>
            _db.CalendarEvents
                .AsNoTracking()
                .Where(e => e.OrganizerId == userId || e.Participants.Any(p => p.UserId == userId))
                .Select(e => new EventSummaryDto(
                    e.Id,
                    e.Type,
                    e.Title,
                    e.StartAt,
                    e.EndAt,
                    e.MeetingUrl,
                    e.OrganizerId,
                    e.Organizer.DisplayName,
                    e.IsCancelled,
                    e.OrganizerId == userId,
                    e.Participants
                        .Where(p => p.UserId == userId)
                        .Select(p => (RsvpStatus?)p.RsvpStatus)
                        .FirstOrDefault()));

        private async Task<CalendarEvent> LoadAsync(int eventId, bool track)
        {
            var query = _db.CalendarEvents
                .Include(e => e.Organizer)
                .Include(e => e.Participants).ThenInclude(p => p.User)
                .AsSplitQuery();

            if (!track)
            {
                query = query.AsNoTracking();
            }

            return await query.FirstOrDefaultAsync(e => e.Id == eventId)
                ?? throw ApiException.NotFound("Запись не найдена.");
        }

        private static void EnsureCanView(CalendarEvent calendarEvent, int userId)
        {
            var isParticipant = calendarEvent.Participants.Any(p => p.UserId == userId);
            if (calendarEvent.OrganizerId != userId && !isParticipant)
            {
                throw ApiException.Forbidden("Нет доступа к этой записи.");
            }
        }

        private static EventDetailsDto ToDetails(CalendarEvent e, int userId)
        {
            var isOrganizer = e.OrganizerId == userId;
            var mine = e.Participants.FirstOrDefault(p => p.UserId == userId);

            // Статусы RSVP видит организатор; участник — только свой.
            var participants = e.Participants
                .OrderBy(p => p.User.DisplayName)
                .Select(p => new ParticipantDto(
                    p.UserId,
                    p.User.DisplayName,
                    p.User.Email,
                    isOrganizer || p.UserId == userId ? p.RsvpStatus : null,
                    isOrganizer || p.UserId == userId ? p.RespondedAt : null))
                .ToList();

            return new EventDetailsDto(
                e.Id,
                e.Type,
                e.Title,
                e.Description,
                e.StartAt,
                e.EndAt,
                e.MeetingUrl,
                new UserDto(e.Organizer.Id, e.Organizer.Email, e.Organizer.DisplayName),
                e.IsCancelled,
                isOrganizer,
                mine?.RsvpStatus,
                participants,
                e.CreatedAt,
                e.UpdatedAt);
        }

        // Проверяет поля по FR-5 и правилам типа записи, возвращает id участников без повторов.
        private async Task<List<int>> ValidateAsync(EventType type, EventRequestBase request, int organizerId)
        {
            var errors = new Dictionary<string, string[]>();

            if (string.IsNullOrWhiteSpace(request.Title))
            {
                errors["title"] = ["Название не должно быть пустым."];
            }

            if (request.EndAt!.Value <= request.StartAt!.Value)
            {
                errors["endAt"] = ["Конец должен быть позже начала."];
            }

            var url = NullIfBlank(request.MeetingUrl);
            if (url is not null)
            {
                if (type == EventType.Event)
                {
                    errors["meetingUrl"] = ["Ссылка есть только у встречи."];
                }
                else if (!url.StartsWith("http://", StringComparison.OrdinalIgnoreCase)
                    && !url.StartsWith("https://", StringComparison.OrdinalIgnoreCase))
                {
                    errors["meetingUrl"] = ["Ссылка должна начинаться с http:// или https://."];
                }
            }

            var participantIds = (request.ParticipantIds ?? []).Distinct().ToList();
            if (type == EventType.Event)
            {
                if (participantIds.Count > 0)
                {
                    errors["participantIds"] = ["У события не бывает участников, создайте встречу."];
                }
            }
            else if (participantIds.Count == 0)
            {
                errors["participantIds"] = ["Укажите хотя бы одного участника."];
            }
            else if (participantIds.Contains(organizerId))
            {
                errors["participantIds"] = ["Организатор не может быть участником."];
            }
            else
            {
                var existing = await _db.Users.CountAsync(u => participantIds.Contains(u.Id));
                if (existing != participantIds.Count)
                {
                    errors["participantIds"] = ["Некоторые участники не найдены."];
                }
            }

            if (errors.Count > 0)
            {
                throw ApiException.Validation(errors);
            }

            return participantIds;
        }

        private static string? NullIfBlank(string? value) =>
            string.IsNullOrWhiteSpace(value) ? null : value.Trim();
    }
}
