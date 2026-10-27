namespace Calendar.Api.Models.Dtos
{
    /// <summary>RsvpStatus виден организатору и самому участнику, остальным — null.</summary>
    public record ParticipantDto(
        int UserId,
        string DisplayName,
        string Email,
        RsvpStatus? RsvpStatus,
        DateTime? RespondedAt);
}
