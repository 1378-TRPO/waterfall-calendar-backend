using System.ComponentModel.DataAnnotations;

namespace Calendar.Api.Models.Dtos
{
    public class RsvpRequest
    {
        [Required]
        public RsvpStatus? Status { get; set; }
    }
}
