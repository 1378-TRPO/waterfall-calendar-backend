namespace Calendar.Api.Models
{
    public class Notification
    {
        public int Id { get; set; }
        public int UserId { get; set; }
        public int? EventId { get; set; }
        public NotificationType Type { get; set; }
        public string Message { get; set; } = string.Empty;
        public bool IsRead { get; set; }
        public DateTime CreatedAt { get; set; }

        public User User { get; set; } = null!;
        public CalendarEvent? Event { get; set; }
    }
}
