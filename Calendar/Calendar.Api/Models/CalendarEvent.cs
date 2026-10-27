namespace Calendar.Api.Models
{
    public class CalendarEvent
    {
        public int Id { get; set; }
        public EventType Type { get; set; }
        public string Title { get; set; } = string.Empty;
        public string? Description { get; set; }
        public DateTime StartAt { get; set; }
        public DateTime EndAt { get; set; }
        public string? MeetingUrl { get; set; }
        public int OrganizerId { get; set; }
        public bool IsCancelled { get; set; }
        public DateTime CreatedAt { get; set; }
        public DateTime UpdatedAt { get; set; }

        public User Organizer { get; set; } = null!;
        public List<EventParticipant> Participants { get; set; } = [];
    }
}
