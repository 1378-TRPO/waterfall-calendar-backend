namespace Calendar.Api.Models.Dtos
{
    public record EventSummaryDto(
        int Id,
        EventType Type,
        string Title,
        DateTime StartAt,
        DateTime EndAt,
        string? MeetingUrl,
        int OrganizerId,
        string OrganizerName,
        bool IsCancelled,
        bool IsOrganizer,
        RsvpStatus? MyRsvpStatus);
}
