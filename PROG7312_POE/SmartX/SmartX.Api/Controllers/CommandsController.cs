
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
        private readonly UserActivityService _activityService;

        public CommandsController(
            CommandStateService stateService,
            CommandHistoryService historyService,
            SensorService sensorService,
            UserActivityService activityService)
        {
            _stateService = stateService;
            _historyService = historyService;
            _sensorService = sensorService;
            _activityService = activityService;
        }

        // Execute a command only for a registered device.
        [HttpPost("execute")]
        public IActionResult ExecuteCommand(
            [FromBody] ManualCommandRequest request)
        {
            // Validate the required command details.
            if (request == null
                || string.IsNullOrWhiteSpace(request.DeviceIdentifier)
                || string.IsNullOrWhiteSpace(request.CommandName)
                || string.IsNullOrWhiteSpace(request.NewValue))
            {
                return BadRequest(new
                {
                    Message = "Device identifier, command name and value are required."
                });
            }

            if (!string.Equals(
                request.CommandName,
                "SetRelayState",
                StringComparison.OrdinalIgnoreCase))
            {
                return BadRequest(new
                {
                    Message = "Unsupported command. Use SetRelayState."
                });
            }

            string deviceIdentifier = request.DeviceIdentifier.Trim();

            // Prevent commands from being sent to unknown devices.
            if (!_sensorService.DeviceExists(deviceIdentifier))
            {
                return NotFound(new
                {
                    Message = $"Device '{deviceIdentifier}' is not registered."
                });
            }

            if (!string.Equals(request.NewValue, "On",
                    StringComparison.OrdinalIgnoreCase)
                && !string.Equals(request.NewValue, "Off",
                    StringComparison.OrdinalIgnoreCase))
            {
                return BadRequest(new
                {
                    Message = "Relay state must be On or Off."
                });
            }

            string commandName = "SetRelayState";

            string previousValue =
                _stateService.GetValue(deviceIdentifier, commandName) ?? "Off";

            string newValue = string.Equals(
                request.NewValue,
                "On",
                StringComparison.OrdinalIgnoreCase) ? "On" : "Off";

            // Update the device's stored command state.
            _stateService.SetValue(
                deviceIdentifier,
                commandName,
                newValue);

            // Push the command onto the stack so it can be undone.
            _historyService.RecordCommand(new ManualCommand
            {
                DeviceIdentifier = deviceIdentifier,
                CommandName = commandName,
                PreviousValue = previousValue,
                NewValue = newValue,
                Timestamp = DateTime.UtcNow
            });

            // Record successful commands so predictions learn from real usage.
            _activityService.RecordCommand(
                deviceIdentifier,
                commandName,
                newValue);

            return Ok(new
            {
                Message = "Command executed successfully.",
                DeviceIdentifier = deviceIdentifier,
                CommandName = commandName,
                PreviousValue = previousValue,
                NewValue = newValue
            });
        }

        // Retrieve the current state of a registered device.
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

            deviceIdentifier = deviceIdentifier.Trim();

            if (!_sensorService.DeviceExists(deviceIdentifier))
            {
                return NotFound(new
                {
                    Message = $"Device '{deviceIdentifier}' is not registered."
                });
            }

            if (string.IsNullOrWhiteSpace(commandName))
            {
                return BadRequest(new
                {
                    Message = "A command name is required."
                });
            }

            string? currentValue = _stateService.GetValue(
                deviceIdentifier,
                commandName.Trim());

            return Ok(new
            {
                DeviceIdentifier = deviceIdentifier,
                CommandName = commandName.Trim(),
                CurrentValue = currentValue ?? "Off"
            });
        }

        // Return commands stored in the undo stack.
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

            // Restore the state recorded before the original command.
            _stateService.SetValue(
                command.DeviceIdentifier,
                command.CommandName,
                command.PreviousValue);

            // Keep undo activity separate from ordinary commands.
            _activityService.RecordUndo(
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

        // Supply the data required by the Command Stream page.
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
