namespace Calendar.Api.Models.Dtos
{
    public record NotificationDto(
        int Id,
        NotificationType Type,
        string Message,
        bool IsRead,
        DateTime CreatedAt,
        int? EventId);
}
