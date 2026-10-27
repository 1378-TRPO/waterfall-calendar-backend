using Calendar.Api.Models.Dtos;
using Calendar.Api.Services;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace Calendar.Api.Controllers
{
    [ApiController]
    [Route("api/notifications")]
    [Authorize]
    public class NotificationsController : ControllerBase
    {
        private readonly INotificationService _notifications;

        public NotificationsController(INotificationService notifications)
        {
            _notifications = notifications;
        }

        [HttpGet]
        public async Task<ActionResult<List<NotificationDto>>> GetAll() =>
            Ok(await _notifications.GetAsync(User.GetUserId()));

        [HttpGet("unread-count")]
        public async Task<ActionResult<UnreadCountDto>> GetUnreadCount() =>
            Ok(await _notifications.GetUnreadCountAsync(User.GetUserId()));

        [HttpPost("{id:int}/read")]
        public async Task<ActionResult<NotificationDto>> MarkRead(int id) =>
            Ok(await _notifications.MarkReadAsync(User.GetUserId(), id));
    }
}
