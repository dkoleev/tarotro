namespace Tarotro.Game.Utils {
    public interface IGameLogger {
        void Info(string message, string category);
        void Warning(string message, string category);
        void Error(string message, string category);
        void Debug(string message, string category);
    }
}
