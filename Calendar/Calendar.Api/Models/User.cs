namespace Calendar.Api.Models
{
    public class User
    {
        public int Id { get; set; }
        public string Email { get; set; } = string.Empty;
        public string PasswordHash { get; set; } = string.Empty;
        public string DisplayName { get; set; } = string.Empty;
        public DateTime CreatedAt { get; set; }

        public List<CalendarEvent> OrganizedEvents { get; set; } = [];
        public List<EventParticipant> Participations { get; set; } = [];
        public List<Notification> Notifications { get; set; } = [];
    }
}
