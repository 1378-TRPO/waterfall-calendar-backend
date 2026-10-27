using System.ComponentModel.DataAnnotations;
using Calendar.Api.Models.Dtos;
using Calendar.Api.Services;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace Calendar.Api.Controllers
{
    [ApiController]
    [Route("api/events")]
    [Authorize]
    public class EventsController : ControllerBase
    {
        private const int DefaultUpcomingLimit = 10;
        private const int MaxUpcomingLimit = 50;

        private readonly IEventService _events;

        public EventsController(IEventService events)
        {
            _events = events;
        }

        [HttpGet]
        public async Task<ActionResult<List<EventSummaryDto>>> GetRange(
            [FromQuery, Required] DateTimeOffset? from, [FromQuery, Required] DateTimeOffset? to) =>
            Ok(await _events.GetRangeAsync(User.GetUserId(), from!.Value, to!.Value));

        [HttpGet("upcoming")]
        public async Task<ActionResult<List<EventSummaryDto>>> GetUpcoming(
            [FromQuery] int limit = DefaultUpcomingLimit) =>
            Ok(await _events.GetUpcomingAsync(User.GetUserId(), Math.Clamp(limit, 1, MaxUpcomingLimit)));

        [HttpGet("{id:int}")]
        public async Task<ActionResult<EventDetailsDto>> Get(int id) =>
            Ok(await _events.GetAsync(User.GetUserId(), id));

        [HttpPost]
        public async Task<ActionResult<EventDetailsDto>> Create(CreateEventRequest request)
        {
            var created = await _events.CreateAsync(User.GetUserId(), request);
            return CreatedAtAction(nameof(Get), new { id = created.Id }, created);
        }

        [HttpPut("{id:int}")]
        public async Task<ActionResult<EventDetailsDto>> Update(int id, UpdateEventRequest request) =>
            Ok(await _events.UpdateAsync(User.GetUserId(), id, request));

        [HttpPost("{id:int}/cancel")]
        public async Task<ActionResult<EventDetailsDto>> Cancel(int id) =>
            Ok(await _events.CancelAsync(User.GetUserId(), id));

        [HttpPut("{id:int}/rsvp")]
        public async Task<ActionResult<EventDetailsDto>> Rsvp(int id, RsvpRequest request) =>
            Ok(await _events.SetRsvpAsync(User.GetUserId(), id, request.Status!.Value));
    }
}
