using System.ComponentModel.DataAnnotations;

namespace Calendar.Api.Models.Dtos
{
    public abstract class EventRequestBase
    {
        [Required, MaxLength(200)]
        public string Title { get; set; } = string.Empty;

        public string? Description { get; set; }

        [Required]
        public DateTimeOffset? StartAt { get; set; }

        [Required]
        public DateTimeOffset? EndAt { get; set; }

        [MaxLength(500)]
        public string? MeetingUrl { get; set; }

        public List<int>? ParticipantIds { get; set; }
    }
}
