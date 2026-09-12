using System.Linq;
using System.Text;
using System.Threading;
using Cysharp.Threading.Tasks;
using Game.Data;
using Game.Logic;

namespace Game.DevConsole.Commands {
    public class SpawnEnemyCommand : IDevConsoleCommand {
        private readonly Battle _battle;
        private readonly GameData _gameData;

        public string Name => "spawnenemy";
        public string Description => "Spawn an enemy by id, or a random one if no id given";
        public string Usage => "spawnenemy [enemy_id]  — use 'spawnenemy list' to see available ids";

        public SpawnEnemyCommand(Battle battle, GameData gameData) {
            _battle = battle;
            _gameData = gameData;
        }

        public string Execute(string[] args) {
            if (args.Length > 0 && args[0] == "list") {
                return ListEnemies();
            }

            if (args.Length == 0) {
                _battle.DevSpawnRandomEnemy(CancellationToken.None).Forget();
                return "Spawning random enemy...";
            }

            var enemyId = args[0];
            if (_gameData.Enemies == null || !_gameData.Enemies.ContainsKey(enemyId)) {
                return $"Unknown enemy id '{enemyId}'. Use 'spawnenemy list' to see available ids.";
            }

            _battle.DevSpawnEnemy(enemyId, CancellationToken.None).Forget();
            return $"Spawning enemy '{enemyId}'...";
        }

        private string ListEnemies() {
            if (_gameData.Enemies == null || _gameData.Enemies.Count == 0) {
                return "No enemies loaded.";
            }

            var sb = new StringBuilder();
            sb.AppendLine("Available enemies:");
            foreach (var e in _gameData.Enemies.Values) {
                sb.AppendLine($"  {e.id} (HP: {e.maxHealth})");
            }

            return sb.ToString().TrimEnd();
        }
    }
}
