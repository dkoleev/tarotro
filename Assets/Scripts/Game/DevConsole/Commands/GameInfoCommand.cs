using System.Text;
using Game.Data;
using UnityEngine;

namespace Game.DevConsole.Commands {
    public class GameInfoCommand : IDevConsoleCommand {
        private readonly GameData _gameData;

        public string Name => "gameinfo";
        public string Description => "Show current game data summary";
        public string Usage => "gameinfo";

        public GameInfoCommand(GameData gameData) {
            _gameData = gameData;
        }

        public string Execute(string[] args) {
            var sb = new StringBuilder();
            sb.AppendLine("=== Game Info ===");
            sb.AppendLine($"Enemies loaded: {_gameData.Enemies?.Count ?? 0}");
            if (_gameData.Enemies != null) {
                foreach (var e in _gameData.Enemies.Values) {
                    sb.AppendLine($"  [{e.id}] HP:{e.maxHealth} Prefab:{e.prefabPath}");
                }
            }
            sb.AppendLine($"Tarot cards loaded: {_gameData.TarotCards?.Count ?? 0}");
            if (_gameData.TarotCards != null) {
                foreach (var c in _gameData.TarotCards.Values) {
                    sb.AppendLine($"  [{c.id}] Index:{c.index} Sprite:{c.spritePath}");
                }
            }
            sb.AppendLine($"Platform: {Application.platform}");
            sb.AppendLine($"Unity: {Application.unityVersion}");
            sb.AppendLine($"Screen: {Screen.width}x{Screen.height}");
            return sb.ToString().TrimEnd();
        }
    }
}
