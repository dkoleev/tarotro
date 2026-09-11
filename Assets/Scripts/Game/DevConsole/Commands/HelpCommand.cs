using System.Text;

namespace Game.DevConsole.Commands {
    public class HelpCommand : IDevConsoleCommand {
        private readonly DevConsole _console;

        public string Name => "help";
        public string Description => "List all available commands";
        public string Usage => "help [command]";

        public HelpCommand(DevConsole console) {
            _console = console;
        }

        public string Execute(string[] args) {
            if (args.Length > 0) {
                var cmdName = args[0].ToLowerInvariant();
                if (_console.Commands.TryGetValue(cmdName, out var cmd)) {
                    return $"{cmd.Name} - {cmd.Description}\nUsage: {cmd.Usage}";
                }
                return $"Unknown command: '{cmdName}'";
            }

            var sb = new StringBuilder();
            sb.AppendLine("Available commands:");
            foreach (var cmd in _console.Commands.Values) {
                sb.AppendLine($"  {cmd.Name,-16} {cmd.Description}");
            }
            return sb.ToString().TrimEnd();
        }
    }
}
