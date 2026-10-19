
using Microsoft.AspNetCore.Mvc;
using SmartX.Api.Models;
using SmartX.Api.Services;

namespace SmartX.Api.Controllers
{
    [ApiController]
    [Route("api/[controller]")]
    public class CommandsController : ControllerBase
    {
        private readonly CommandStateService _stateService;
        private readonly CommandHistoryService _historyService;

        public CommandsController(
            CommandStateService stateService,
            CommandHistoryService historyService)
        {
            _stateService = stateService;
            _historyService = historyService;
        }

        [HttpPost("execute")]
        public IActionResult ExecuteCommand(
            [FromBody] ManualCommandRequest request)
        {
            // This first version supports relay on/off commands.
            if (!request.CommandName.Equals(
                "SetRelayState",
                StringComparison.OrdinalIgnoreCase))
            {
                return BadRequest(new
                {
                    Message = "Unsupported command. Use SetRelayState."
                });
            }

            if (string.IsNullOrWhiteSpace(request.DeviceIdentifier))
            {
                return BadRequest(new
                {
                    Message = "A device identifier is required."
                });
            }

            if (!request.NewValue.Equals("On",
                    StringComparison.OrdinalIgnoreCase)
                && !request.NewValue.Equals("Off",
                    StringComparison.OrdinalIgnoreCase))
            {
                return BadRequest(new
                {
                    Message = "Relay state must be On or Off."
                });
            }

            string deviceIdentifier = request.DeviceIdentifier.Trim();
            string commandName = "SetRelayState";

            // A relay starts in the Off state if no value has been recorded.
            string previousValue =
                _stateService.GetValue(deviceIdentifier, commandName) ?? "Off";

            string newValue = request.NewValue.Equals(
                "On", StringComparison.OrdinalIgnoreCase)
                ? "On"
                : "Off";

            // Update the device state.
            _stateService.SetValue(
                deviceIdentifier,
                commandName,
                newValue);

            // Save the previous and new values so we can undo this command.
            _historyService.RecordCommand(new ManualCommand
            {
                DeviceIdentifier = deviceIdentifier,
                CommandName = commandName,
                PreviousValue = previousValue,
                NewValue = newValue,
                Timestamp = DateTime.UtcNow
            });

            return Ok(new
            {
                Message = "Command executed successfully.",
                DeviceIdentifier = deviceIdentifier,
                CommandName = commandName,
                PreviousValue = previousValue,
                NewValue = newValue
            });
        }

        [HttpGet("state")]
        public IActionResult GetCommandState(
            string deviceIdentifier,
            string commandName = "SetRelayState")
        {
            if (string.IsNullOrWhiteSpace(deviceIdentifier))
            {
                return BadRequest(new
                {
                    Message = "A device identifier is required."
                });
            }

            string? currentValue = _stateService.GetValue(
                deviceIdentifier.Trim(),
                commandName);

            return Ok(new
            {
                DeviceIdentifier = deviceIdentifier.Trim(),
                CommandName = commandName,
                CurrentValue = currentValue ?? "Off"
            });
        }

        [HttpGet("history")]
        public IActionResult GetCommandHistory()
        {
            // Stack.ToArray returns the newest command first.
            return Ok(_historyService.GetHistory());
        }
    }
}
