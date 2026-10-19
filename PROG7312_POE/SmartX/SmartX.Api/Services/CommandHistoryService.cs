using SmartX.Api.Models;

namespace SmartX.Api.Services
{
    public class CommandHistoryService
    {
        // The most recent command is stored at the top.
        private readonly Stack<ManualCommand> _history = new();

        public int Count => _history.Count;

        public void RecordCommand(ManualCommand command)
        {
            _history.Push(command);
        }

        public bool TryUndo(out ManualCommand? command)
        {
            // Removes the most recent command from the history.
            return _history.TryPop(out command);
        }

        public ManualCommand[] GetHistory()
        {
            // Returns a copy of the history without removing commands.
            return _history.ToArray();
        }
    }
}