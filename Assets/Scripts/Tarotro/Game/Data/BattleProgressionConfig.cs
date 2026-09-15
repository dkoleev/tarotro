using System;

namespace Tarotro.Game.Data {
    [Serializable]
    public class BattleProgressionConfig {
        public int baseScore;
        public float scalingFactor;
        public float smallBlindMultiplier;
        public float bigBlindMultiplier;
        public float bossBlindMultiplier;
    }
}
