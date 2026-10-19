
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

            _stateService.SetValue(
                deviceIdentifier,
                commandName,
                newValue);

            // Save the command so it can be undone later.
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
            // The most recent command appears first.
            return Ok(_historyService.GetHistory());
        }

        [HttpPost("undo")]
        public IActionResult UndoLastCommand()
        {
            // TryPop safely handles an empty stack.
            if (!_historyService.TryUndo(out var command)
                || command == null)
            {
                return NotFound(new
                {
                    Message = "There are no commands available to undo."
                });
            }

            // Restore the state that existed before the command.
            _stateService.SetValue(
                command.DeviceIdentifier,
                command.CommandName,
                command.PreviousValue);

            return Ok(new
            {
                Message = "The last command was undone successfully.",
                DeviceIdentifier = command.DeviceIdentifier,
                CommandName = command.CommandName,
                RestoredValue = command.PreviousValue,
                UndoneValue = command.NewValue
            });
        }
    }
}
