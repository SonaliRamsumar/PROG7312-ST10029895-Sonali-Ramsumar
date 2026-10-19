
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
        private readonly PredictiveActionService _predictiveActionService;

        public ActivityController(
            UserActivityService activityService,
            PredictiveActionService predictiveActionService)
        {
            _activityService = activityService;
            _predictiveActionService = predictiveActionService;
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

        // View recorded searches and commands.
        [HttpGet("history")]
        public IActionResult GetActivityHistory()
        {
            return Ok(_activityService.GetAll());
        }

        // Generate suggestions from previously recorded activity.
        [HttpGet("predictions")]
        public IActionResult GetPredictions()
        {
            return Ok(_predictiveActionService.GetPredictions());
        }
    }
}
