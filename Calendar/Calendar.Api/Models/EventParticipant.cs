namespace Calendar.Api.Models
{
    public class EventParticipant
    {
        public int EventId { get; set; }
        public int UserId { get; set; }
        public RsvpStatus RsvpStatus { get; set; } = RsvpStatus.Pending;
        public DateTime? RespondedAt { get; set; }

        public CalendarEvent Event { get; set; } = null!;
        public User User { get; set; } = null!;
    }
}
