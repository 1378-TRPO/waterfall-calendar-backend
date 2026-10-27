namespace Calendar.Api.Models.Dtos
{
    public record EventDetailsDto(
        int Id,
        EventType Type,
        string Title,
        string? Description,
        DateTime StartAt,
        DateTime EndAt,
        string? MeetingUrl,
        UserDto Organizer,
        bool IsCancelled,
        bool IsOrganizer,
        RsvpStatus? MyRsvpStatus,
        List<ParticipantDto> Participants,
        DateTime CreatedAt,
        DateTime UpdatedAt);
}
