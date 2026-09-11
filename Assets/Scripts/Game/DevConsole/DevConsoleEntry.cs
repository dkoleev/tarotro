using System;

namespace Game.DevConsole {
    public enum DevConsoleEntryType {
        Info,
        Warning,
        Error,
        Command,
        Result
    }

    public readonly struct DevConsoleEntry {
        public string Message { get; }
        public string Category { get; }
        public DevConsoleEntryType Type { get; }
        public DateTime Timestamp { get; }

        public DevConsoleEntry(string message, string category, DevConsoleEntryType type) {
            Message = message;
            Category = category;
            Type = type;
            Timestamp = DateTime.Now;
        }
    }
}
