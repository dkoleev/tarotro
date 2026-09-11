namespace Game.DevConsole.Commands {
    public class ClearCommand : IDevConsoleCommand {
        private readonly DevConsole _console;

        public string Name => "clear";
        public string Description => "Clear the console output";
        public string Usage => "clear";

        public ClearCommand(DevConsole console) {
            _console = console;
        }

        public string Execute(string[] args) {
            _console.ClearEntries();
            return null;
        }
    }
}
