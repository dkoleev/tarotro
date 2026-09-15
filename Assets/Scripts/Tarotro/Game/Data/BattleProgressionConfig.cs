using System;

namespace Tarotro.Game.Data {
    [Serializable]
    public class BattleProgressionConfig {
        public int baseScore;
        public float scalingFactor;
        public float commonEnemyMult;
        public float eliteEnemyMult;
        public float bossEnemyMult;
    }
}
