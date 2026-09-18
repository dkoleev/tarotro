using Tarotro.Game.Utils;

namespace Tarotro.Tests {
    public class NullLogger : IGameLogger {
        public void Info(string message, string category) { }
        public void Warning(string message, string category) { }
        public void Error(string message, string category) { }
        public void Debug(string message, string category) { }
    }
}
