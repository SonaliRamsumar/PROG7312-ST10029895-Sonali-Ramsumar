
using Microsoft.AspNetCore.Mvc;
using SmartX.Api.Models;
using SmartX.Api.Services;

namespace SmartX.Api.Controllers
{
    [ApiController]
    [Route("api/[controller]")]
    public class ActivityController : ControllerBase
    {
        private readonly UserActivityService _activityService;

        public ActivityController(UserActivityService activityService)
        {
            _activityService = activityService;
        }

        // Record a search performed in the Command Stream.
        [HttpPost("search")]
        public IActionResult RecordSearch(
            [FromBody] SearchActivityRequest request)
        {
            _activityService.RecordSearch(
                request.Query,
                request.Category,
                request.StateFilter);

            return Ok(new
            {
                Message = "Search activity received."
            });
        }

        // View recorded searches and commands during development.
        [HttpGet("history")]
        public IActionResult GetActivityHistory()
        {
            return Ok(_activityService.GetAll());
        }
    }
}
