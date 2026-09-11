namespace Game.DevConsole {
    public interface IDevConsoleCommand {
        string Name { get; }
        string Description { get; }
        string Usage { get; }
        string Execute(string[] args);
    }
}
