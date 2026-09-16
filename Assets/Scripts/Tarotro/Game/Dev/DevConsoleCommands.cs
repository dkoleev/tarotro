using System.Text;
using System.Threading;
using Cysharp.Threading.Tasks;
using JetBrains.Annotations;
using QFSW.QC;
using Tarotro.Game.Data;
using Tarotro.Game.Logic;
using Tarotro.Game.Utils;
using VContainer;

namespace Tarotro.Game.Dev {
    public class DevConsoleCommands {
        private readonly Battle _battle;
        private readonly GameData _gameData;
        private readonly IGameLogger _gameLogger;

        [Inject]
        public DevConsoleCommands(Battle battle, GameData gameData, IGameLogger gameLogger) {
            _battle = battle;
            _gameData = gameData;
            _gameLogger = gameLogger;
        }

        [UsedImplicitly]
        [Command("spawn-enemy", MonoTargetType.Registry)]
        public void SpawnEnemy(string enemyId, int health = 100) {
            if (_gameData.Enemies == null || !_gameData.Enemies.ContainsKey(enemyId)) {
                _gameLogger.Warn($"Unknown enemy id '{enemyId}'. Use 'list-enemies' to see available ids.", "dev");
                return;
            }
            _battle.SpawnEnemy(enemyId, health, CancellationToken.None).Forget();
        }
        
        [UsedImplicitly]
        [Command("spawn-random-enemy", MonoTargetType.Registry)]
        public void SpawnRandomEnemy(int health = 100) {
            _battle.SpawnRandomEnemy(health, CancellationToken.None).Forget();
        }
        
        [UsedImplicitly]
        [Command("player-attack", MonoTargetType.Registry)]
        public void PlayerAttack(int damage) {
            _battle.PlayerAttack(damage);
        }
        
        [UsedImplicitly]
        [Command("list-enemies", MonoTargetType.Registry)]
        private void ListEnemies() {
            if (_gameData.Enemies == null || _gameData.Enemies.Count == 0) {
                _gameLogger.Info("No enemies loaded.", "dev");
                return;
            }

            var sb = new StringBuilder();
            sb.AppendLine("Available enemies:");
            foreach (var e in _gameData.Enemies.Values) {
                sb.AppendLine($"  {e.id} (Type: {e.type})");
            }

            _gameLogger.Info(sb.ToString().TrimEnd(), "dev");
        }
    }
}