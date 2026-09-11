using System;
using System.Collections.Generic;

namespace Game.DevConsole {
    public class DevConsole {
        private const int MaxLogEntries = 500;

        private readonly Dictionary<string, IDevConsoleCommand> _commands = new();
        private readonly List<DevConsoleEntry> _entries = new();

        public IReadOnlyList<DevConsoleEntry> Entries => _entries;
        public event Action<DevConsoleEntry> EntryAdded;

        public void RegisterCommand(IDevConsoleCommand command) {
            _commands[command.Name.ToLowerInvariant()] = command;
        }

        public IReadOnlyDictionary<string, IDevConsoleCommand> Commands => _commands;

        public void ExecuteCommand(string input) {
            if (string.IsNullOrWhiteSpace(input)) return;

            AddEntry(new DevConsoleEntry("> " + input, DevConsoleEntryType.Command));

            var parts = input.Trim().Split(' ', StringSplitOptions.RemoveEmptyEntries);
            var commandName = parts[0].ToLowerInvariant();
            var args = parts.Length > 1 ? parts[1..] : Array.Empty<string>();

            if (!_commands.TryGetValue(commandName, out var command)) {
                AddEntry(new DevConsoleEntry($"Unknown command: '{commandName}'. Type 'help' for available commands.", DevConsoleEntryType.Error));
                return;
            }

            try {
                var result = command.Execute(args);
                if (!string.IsNullOrEmpty(result)) {
                    AddEntry(new DevConsoleEntry(result, DevConsoleEntryType.Result));
                }
            } catch (Exception e) {
                AddEntry(new DevConsoleEntry($"Command error: {e.Message}", DevConsoleEntryType.Error));
            }
        }

        public void ClearEntries() {
            _entries.Clear();
        }

        private void AddEntry(DevConsoleEntry entry) {
            _entries.Add(entry);
            if (_entries.Count > MaxLogEntries) {
                _entries.RemoveAt(0);
            }
            EntryAdded?.Invoke(entry);
        }
    }
}
