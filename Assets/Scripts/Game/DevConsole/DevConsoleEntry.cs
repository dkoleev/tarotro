using System;

namespace Game.DevConsole {
    public enum DevConsoleEntryType {
        Command,
        Result,
        Error
    }

    public readonly struct DevConsoleEntry {
        public string Message { get; }
        public DevConsoleEntryType Type { get; }
        public DateTime Timestamp { get; }

        public DevConsoleEntry(string message, DevConsoleEntryType type) {
            Message = message;
            Type = type;
            Timestamp = DateTime.Now;
        }
    }
}
