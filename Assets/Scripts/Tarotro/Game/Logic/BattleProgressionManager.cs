using System;
using System.Collections.Generic;
using System.Linq;
using Tarotro.Game.Data;
using Tarotro.Game.Logic.Rng;
using Tarotro.Game.Utils;
using VContainer;

namespace Tarotro.Game.Logic {
    public class BattleProgressionManager {
        private readonly GameData _gameData;
        private readonly GameRng _rng;
        private readonly IGameLogger _logger;

        [Inject]
        public BattleProgressionManager(GameData gameData, GameRng rng, IGameLogger logger) {
            _gameData = gameData;
            _rng = rng;
            _logger = logger;
        }

        public int CalculateTargetScore(CircleType circleType, EnemyType enemyType) {
            var circleIndex = _gameData.Circles[circleType].index;
            var config = _gameData.Battle.progression;
            var blindMultiplier = GetBlindMultiplier(enemyType);
            var exactScore = config.baseScore * Math.Pow(config.scalingFactor, circleIndex - 1) * blindMultiplier;
            return (int)(Math.Round(exactScore / 50.0) * 50);
        }

        public FightRoundData GenerateRound(CircleType circleType, EnemyType enemyType) {
            var targetScore = CalculateTargetScore(circleType, enemyType);
            var enemy = SelectEnemy(circleType, enemyType);

            _logger.Info($"Generated round: Circle {circleType}, {enemyType}, Target: {targetScore}, Enemy: {enemy.id}", "BattleProgression");

            return new FightRoundData {
                Circle = circleType,
                EnemyType = enemyType,
                TargetScore = targetScore,
                EnemyId = enemy.id
            };
        }

        public List<FightRoundData> GenerateCircle(CircleType circleType) {
            var result = new List<FightRoundData>();
            var circleData = _gameData.Circles[circleType];

            if (circleData == null) {
                _logger.Error($"Circle data not found for {circleType}", "BattleProgression");
                return result;
            }

            foreach (var step in circleData.steps) {
                result.Add(GenerateRound(circleType, step));
            }

            return result;
        }

        private EnemyData SelectEnemy(CircleType circleType, EnemyType enemyType) {
            var circleData = _gameData.Circles[circleType];

            if (circleData == null) {
                _logger.Error($"Circle data not found for {circleType}", "BattleProgression");
                return _gameData.Enemies.Values.FirstOrDefault();
            }

            var filteredEnemies = new List<EnemyData>();
            foreach (var enemyData in _gameData.Enemies.Values) {
                if (enemyData.type == enemyType && circleData.enemies.Contains(enemyData.id)) {
                    filteredEnemies.Add(enemyData);
                }
            }

            if (filteredEnemies.Count == 0) {
                _logger.Error($"No enemies found for circle {circleType}, type {enemyType}", "BattleProgression");
                return _gameData.Enemies.Values.FirstOrDefault();
            }

            return filteredEnemies[_rng.Range(RngChannel.EnemySelect, 0, filteredEnemies.Count)];
        }

        private float GetBlindMultiplier(EnemyType enemyType) {
            var config = _gameData.Battle.progression;
            return enemyType switch {
                EnemyType.Common => config.commonEnemyMult,
                EnemyType.Elite => config.eliteEnemyMult,
                EnemyType.Boss => config.bossEnemyMult,
                _ => 1f
            };
        }
    }
}
