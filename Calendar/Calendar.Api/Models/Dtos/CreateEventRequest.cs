using System.ComponentModel.DataAnnotations;

namespace Calendar.Api.Models.Dtos
{
    public class CreateEventRequest : EventRequestBase
    {
        [Required]
        public EventType? Type { get; set; }
    }
}
