using System;
using System.Collections.Generic;
using System.Linq;
using Tarotro.Game.Data;
using Tarotro.Game.Utils;
using Random = UnityEngine.Random;

namespace Tarotro.Game.Logic {
    public class BattleProgressionManager {
        private readonly GameData _gameData;
        private readonly IGameLogger _logger;

        public BattleProgressionManager(GameData gameData, IGameLogger logger) {
            _gameData = gameData;
            _logger = logger;
        }

        public int CalculateTargetScore(int circle, BlindType blindType) {
            var config = _gameData.Battle.progression;
            var blindMultiplier = GetBlindMultiplier(blindType);
            var exactScore = config.baseScore * Math.Pow(config.scalingFactor, circle - 1) * blindMultiplier;
            return (int)(Math.Round(exactScore / 50.0) * 50);
        }

        public FightRoundData GenerateRound(int circle, BlindType blindType) {
            var targetScore = CalculateTargetScore(circle, blindType);
            var enemy = SelectEnemy();

            _logger.Info($"Generated round: Circle {circle}, {blindType}, Target: {targetScore}, Enemy: {enemy.id}", "BattleProgression");

            return new FightRoundData {
                Circle = circle,
                BlindType = blindType,
                TargetScore = targetScore,
                EnemyId = enemy.id
            };
        }

        public List<FightRoundData> GenerateCircle(int circle) {
            return new List<FightRoundData> {
                GenerateRound(circle, BlindType.SmallBlind),
                GenerateRound(circle, BlindType.BigBlind),
                GenerateRound(circle, BlindType.BossBlind)
            };
        }

        private EnemyData SelectEnemy() {
            var enemies = _gameData.Enemies.Values.ToList();
            return enemies[Random.Range(0, enemies.Count)];
        }

        private float GetBlindMultiplier(BlindType blindType) {
            var config = _gameData.Battle.progression;
            return blindType switch {
                BlindType.SmallBlind => config.smallBlindMultiplier,
                BlindType.BigBlind => config.bigBlindMultiplier,
                BlindType.BossBlind => config.bossBlindMultiplier,
                _ => 1f
            };
        }
    }
}
