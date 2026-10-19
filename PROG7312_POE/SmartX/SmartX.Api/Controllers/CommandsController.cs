
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
        private readonly SensorService _sensorService;

        public CommandsController(
            CommandStateService stateService,
            CommandHistoryService historyService,
            SensorService sensorService)
        {
            _stateService = stateService;
            _historyService = historyService;
            _sensorService = sensorService;
        }

        // Execute a manual relay command and save it for undo.
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

            if (!request.NewValue.Equals(
                    "On", StringComparison.OrdinalIgnoreCase)
                && !request.NewValue.Equals(
                    "Off", StringComparison.OrdinalIgnoreCase))
            {
                return BadRequest(new
                {
                    Message = "Relay state must be On or Off."
                });
            }

            string deviceIdentifier = request.DeviceIdentifier.Trim();
            string commandName = "SetRelayState";

            // Treat a relay with no saved state as Off.
            string previousValue =
                _stateService.GetValue(deviceIdentifier, commandName) ?? "Off";

            string newValue = request.NewValue.Equals(
                "On", StringComparison.OrdinalIgnoreCase) ? "On" : "Off";

            _stateService.SetValue(
                deviceIdentifier, commandName, newValue);

            // Push the command onto the stack so it can be undone.
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

        // Get the current state of one device command.
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
                deviceIdentifier.Trim(), commandName);

            return Ok(new
            {
                DeviceIdentifier = deviceIdentifier.Trim(),
                CommandName = commandName,
                CurrentValue = currentValue ?? "Off"
            });
        }

        // Get the commands that are still in the undo history stack.
        [HttpGet("history")]
        public IActionResult GetCommandHistory()
        {
            return Ok(_historyService.GetHistory());
        }

        // Undo the most recently executed command.
        [HttpPost("undo")]
        public IActionResult UndoLastCommand()
        {
            if (!_historyService.TryUndo(out var command)
                || command == null)
            {
                return NotFound(new
                {
                    Message = "There are no commands available to undo."
                });
            }

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

        // Supply the devices, current command states, and history
        // together for the Command Stream frontend page.
        [HttpGet("stream")]
        public IActionResult GetCommandStream()
        {
            return Ok(new
            {
                Devices = _sensorService.GetAll(),
                CommandStates = _stateService.GetAllStates(),
                CommandHistory = _historyService.GetHistory()
            });
        }
    }
}
