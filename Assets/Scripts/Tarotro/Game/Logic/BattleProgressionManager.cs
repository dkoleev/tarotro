using System;
using System.Collections.Generic;
using System.Linq;
using Tarotro.Game.Data;
using Tarotro.Game.Utils;
using VContainer;
using Random = UnityEngine.Random;

namespace Tarotro.Game.Logic {
    public class BattleProgressionManager {
        private readonly GameData _gameData;
        private readonly IGameLogger _logger;

        [Inject]
        public BattleProgressionManager(GameData gameData, IGameLogger logger) {
            _gameData = gameData;
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
            var result  = new List<FightRoundData>();
            foreach (var step in _gameData.Circles[circleType].steps) {
                result.Add(GenerateRound(circleType, step));
            }

            return result;
        }

        private EnemyData SelectEnemy(CircleType circleType, EnemyType enemyType) {
            var circleData = _gameData.Circles[circleType];
            var filteredEnemies = new List<EnemyData>();
            foreach (var enemyData in _gameData.Enemies.Values) {
                if (enemyData.type == enemyType && circleData.enemies.Contains(enemyData.id)) {
                    filteredEnemies.Add(enemyData);
                }    
            }
            
            return filteredEnemies[Random.Range(0, filteredEnemies.Count)];
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
